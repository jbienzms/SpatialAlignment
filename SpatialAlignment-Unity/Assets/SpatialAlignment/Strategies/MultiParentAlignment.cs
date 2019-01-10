//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// Defines the various modes that <see cref="MultiParentAlignment"/> can
    /// use to select and apply parent options.
    /// </summary>
    public enum MultiParentMode
    {
        /// <summary>
        /// Only values from the single closest valid parent will be used.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This mode has the least impact to application performance, however
        /// "popping" may be visible when switching between parents. In
        /// addition, a larger number of parent options may be required to
        /// achieve the desired accuracy. This is especially true over larger
        /// distances.
        /// </para>
        /// <para>
        /// This mode actually parents the transform of the current object to
        /// the transform of the closest parent. Because of this,
        /// <see cref="MultiParentBase.UpdateFrequency">UpdateFrequency</see>
        /// can usually be set to higher (longer) values.
        /// </para>
        /// <para>
        /// For example: If the parent is a QR code, the parent's transform
        /// will be updated automatically by the QR tracking system
        /// independently of
        /// <see cref="MultiParentBase.UpdateFrequency">UpdateFrequency</see>.
        /// Keep in mind though that
        /// <see cref="MultiParentBase.UpdateFrequency">UpdateFrequency</see>
        /// is still used to determine which parent is closest as the user
        /// moves through the environment.
        /// </para>
        /// </remarks>
        Closest,

        /// <summary>
        /// The values from all valid parents are applied using weights that
        /// are based on distance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This mode can offer a much higher level of accuracy, especially
        /// when there are fewer parent options or those options are distributed
        /// across larger distances. This mode requires additional computation
        /// to evaluate the state of all parents and to compute the weighted
        /// values for each.
        /// </para>
        /// <para>
        /// This mode directly updates the World transform of the attached
        /// object and therefore generally requires smaller (shorter) values
        /// for
        /// <see cref="MultiParentBase.UpdateFrequency">UpdateFrequency</see>.
        /// However, in MRTK builds an Interpolator is used to animate values
        /// over time which can help with a perception of responsiveness even
        /// over longer update periods.
        /// </para>
        /// </remarks>
        DistanceWeighted
    }

    /// <summary>
    /// A strategy that coordinates alignment based on the alignment of multiple
    /// "parent" objects.
    /// </summary>
    /// <remarks>
    /// Depending on the <see cref="Mode"/>, this strategy may directly update
    /// the World transform of the attached object.
    /// </remarks>
    [DataContract]
    public class MultiParentAlignment : MultiParentBase
    {
        #region Nested Types
        /// <summary>
        /// Used when calculating parent weights.
        /// </summary>
        private class WeightedParent
        {
            public ParentAlignmentOptions Option;
            public double InverseDistanceWeight;
            public float Weight;
        }
        #endregion // Nested Types

        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Range(0.1f, 4.0f)]
        [Tooltip("The power factor used for calculating weights in DistanceWeighted mode. The default is 2.0, which results in inverse-squared weighting.")]
        float distancePower = 2f;

        [DataMember]
        [SerializeField]
        [Tooltip("The mode used for selecting and applying parent options.")]
        MultiParentMode mode;
        #endregion // Unity Inspector Variables

        #region Overrides / Event Handlers
        /// <inheritdoc />
        protected override void ApplyParents(bool force = false)
        {
            // If there are no options, warn and bail
            if (CurrentParents.Count < 1)
            {
                State = AlignmentState.Inhibited;
                Debug.LogWarning($"{nameof(MultiParentAlignment)}: No {nameof(CurrentParents)} available to apply.");
                return;
            }

            // If there is only one, apply it immediately
            if (CurrentParents.Count == 1)
            {
                // Get single parent
                ParentAlignmentOptions parentOption = CurrentParents[0];

                // Make our transform a child of the frame
                this.transform.SetParent(parentOption.Frame.transform, worldPositionStays: false);

                // Apply transform modifications locally
                this.transform.localPosition = parentOption.Position;
                this.transform.localRotation = Quaternion.Euler(parentOption.Rotation);
                this.transform.localScale = parentOption.Scale;
            }
            else
            {
                // Attempt to get the reference transform
                Transform reference = GetReferenceTransform();

                // If no reference transform available, we can't continue
                if (reference == null)
                {
                    State = AlignmentState.Inhibited;
                    Debug.LogWarning($"{nameof(MultiParentAlignment)}: No reference transform could be determined.");
                    return;
                }

                // Placeholders
                Vector3 weightedPos = Vector3.zero;
                Vector3 weightedRotation = Vector3.zero;
                Vector3 weightedScale = Vector3.zero;

                // Use inspector-configurable power factor for Inverse Distance Weighting
                Double power = distancePower;

                // Calculate inverse distance weights for each option
                var weightedParents = (from option in CurrentParents
                                       select new WeightedParent()
                {
                    Option = option,
                    InverseDistanceWeight = 1d / Math.Pow(power, option.DistanceSort(reference)),
                }).ToList();

                // Get total overall inverse distance weight
                Double totalIDW = weightedParents.Sum(o => o.InverseDistanceWeight);

                // Calculate individual weights
                weightedParents.ForEach(o => o.Weight = (float)(o.InverseDistanceWeight / totalIDW));

                // Calculate weighted offsets
                weightedParents.ForEach(o =>
                {
                    weightedPos += (o.Option.Frame.transform.position + o.Option.Position).Weighted(o.Weight);
                    weightedRotation += (o.Option.Frame.transform.rotation.eulerAngles + o.Option.Rotation).Weighted(o.Weight);
                    weightedScale += (Vector3.Scale(o.Option.Frame.transform.localScale, o.Option.Scale)).Weighted(o.Weight);
                });

                // Set no parent
                this.transform.parent = null;

                // Apply offsets globally and animated
                this.transform.AnimateTo(weightedPos, Quaternion.Euler(weightedRotation), weightedScale);
            }

            // Done!
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            // If we have an interpolator running, finish immediately
            this.transform.EndAnimation();

            // Pass on to base
            base.OnDisable();
        }

        /// <inheritdoc />
        protected override List<ParentAlignmentOptions> SelectParents()
        {
            switch (mode)
            {
                case MultiParentMode.Closest:
                    return (from o in ParentOptions
                            where IsValidParent(o)
                            orderby o.DistanceSort(GetReferenceTransform())
                            select o).Take(1).ToList();

                case MultiParentMode.DistanceWeighted:
                    return (from o in ParentOptions
                            where IsValidParent(o)
                            select o).ToList();

                default:
                    throw new InvalidOperationException($"Unknown mode: {mode}");
            }
        }
        #endregion // Overrides / Event Handlers

        #region Public Properties
        /// <summary>
        /// Gets or sets the power factor used for calculating weights in
        /// <see cref="MultiParentMode.DistanceWeighted">DistanceWeighted</see>
        /// mode.
        /// </summary>
        /// <remarks>
        /// The default is 2.0, which results in inverse-squared weighting.
        /// Changing this value will cause the transform to be recalculated.
        /// </remarks>
        public float DistancePower
        {
            get
            {
                return distancePower;
            }
            set
            {
                // Ensure changing
                if (distancePower != value)
                {
                    // Store
                    distancePower = value;

                    // Attempt to update the transform
                    UpdateTransform();
                }
            }
        }

        /// <summary>
        /// Gets or sets the mode for selecting and applying parent options.
        /// </summary>
        /// <remarks>
        /// Changing this value will cause the transform to be recalculated.
        /// </remarks>
        public MultiParentMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                // Ensure changing
                if (mode != value)
                {
                    // Store
                    mode = value;

                    // Attempt to update the transform
                    UpdateTransform();
                }
            }
        }
        #endregion // Public Properties
    }
}