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
        /// Only the closest valid parent will be applied.
        /// </summary>
        /// <remarks>
        /// This mode has the lowest system requirements, but "hopping" may be
        /// noticeable when switching between parents.
        /// </remarks>
        Closest,
        /// <summary>
        /// All valid parents are applied using weighted values. The weights
        /// are calculated based on the distance from the user.
        /// </summary>
        /// <remarks>
        /// This mode offers the highest accuracy at the cost of increased
        /// calculations.
        /// </remarks>
        DistanceWeighted
    }

    /// <summary>
    /// A strategy that coordinates alignment based on the alignment of multiple "parent" objects.
    /// </summary>
    /// <remarks>
    /// This strategy directly updates the world transform of the attached object. Therefore,
    /// the transform of the Unity parent GameObject has no impact on alignment unless it is
    /// added to the <see cref="ParentOptions"/> collection.
    /// </remarks>
    [DataContract]
    public class MultiParentAlignment : AlignmentStrategy
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

        #region Member Variables
        private List<ParentAlignmentOptions> currentParents;
        private float lastUpdateTime;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Tooltip("The mode used for selecting and applying parent options.")]
        MultiParentMode mode;

        [DataMember]
        [SerializeField]
        [Tooltip("The list of parent alignment options.")]
        private List<ParentAlignmentOptions> parentOptions = new List<ParentAlignmentOptions>();

        [SerializeField]
        [Tooltip("The transform that will serve as the frame of reference when calculating modes like NearestNeighbor. If blank, the main camera transform will be used.")]
        private Transform referenceTransform;

        [DataMember]
        [SerializeField]
        [Tooltip("The time between updates (in seconds). If zero, alignment is updated every frame.")]
        private float updateFrequency = 2.00f;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Applies <see cref="CurrentParents"/> transforms to the current object.
        /// </summary>
        /// <param name="force">
        /// <c>true</c> to force an update even if cached information remains
        /// unchanged.
        /// The default is <c>false</c>.
        /// </param>
        /// <remarks>
        /// The default implementation of this method parents the transform
        /// and applies position, rotation and scale modifications.
        /// </remarks>
        protected virtual void ApplyParents(bool force=false)
        {
            // If there are no options, warn and bail
            if (currentParents.Count < 1)
            {
                State = AlignmentState.Inhibited;
                Debug.LogWarning($"{nameof(MultiParentAlignment)}: No {nameof(CurrentParents)} available to apply.");
                return;
            }

            // If there is only one, apply it immediately
            if (currentParents.Count == 1)
            {
                // Get single parent
                ParentAlignmentOptions parentOption = currentParents[0];

                // Make our transform a child of the frame
                this.transform.SetParent(parentOption.Frame.transform, worldPositionStays: false);

                // Apply transform modifications
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

                // Determine a power factor to use for Inverse Distance Weighting
                Double power = 2d;

                // Calculate inverse distance weights for each option
                var weightedParents = (from option in currentParents
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
                    weightedScale += (o.Option.Frame.transform.localScale + o.Option.Scale).Weighted(o.Weight);
                });

                // Set no parent
                this.transform.parent = null;

                // Apply offsets
                this.transform.position = weightedPos;
                this.transform.rotation = Quaternion.Euler(weightedRotation);
                this.transform.localScale = weightedScale;
            }

            // Done!
        }

        /// <summary>
        /// Gets the transform that serves as the frame of reference.
        /// </summary>
        /// <returns>
        /// The reference transform, if one can be determined;
        /// otherwise <see langword = "null" />.
        /// </returns>
        /// <remarks>
        /// The default implementation returns the value of
        /// <see cref="ReferenceTransform"/> if set; otherwise it
        /// attempts to return the main camera transform.
        /// </remarks>
        protected virtual Transform GetReferenceTransform()
        {
            if (referenceTransform == null)
            {
                referenceTransform = Camera.main?.transform;
            }
            return referenceTransform;
        }

        /// <summary>
        /// Selects the best parent options to use from the list of all
        /// available <see cref="ParentOptions"/>.
        /// </summary>
        /// <returns>
        /// The list of <see cref="ParentAlignmentOptions"/> that represent
        /// the best available options.
        /// </returns>
        /// <remarks>
        /// The default implementation selects options based on the current
        /// <see cref="Mode"/> setting.
        /// </remarks>
        protected virtual List<ParentAlignmentOptions> SelectParents()
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

        /// <summary>
        /// Validates the contents of the <see cref="ParentOptions"/> collection.
        /// </summary>
        /// <remarks>
        /// The default implementation simply validates that all parent options have a valid parent.
        /// </remarks>
        protected virtual void ValidateParentOptions()
        {
            // Validate each option
            for (int i = 0; i < parentOptions.Count; i++)
            {
                var opt = parentOptions[i];
                if (opt.Frame == null) { throw new InvalidOperationException($"{nameof(ParentAlignmentOptions)}.{nameof(ParentAlignmentOptions.Frame)} can't be null."); }
                if (opt.Frame.transform == null) { throw new InvalidOperationException($"{nameof(SpatialFrame)}.{nameof(SpatialFrame.transform)} can't be null."); }
            }
        }

        /// <summary>
        /// Returns a value that indicates if <see cref="UpdateTransform"/>
        /// should be called, usually by the <see cref="Update"/> loop.
        /// </summary>
        /// <returns>
        /// <c>true</c> if <see cref="UpdateTransform"/> should be called;
        /// otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The default implementation returns <c>true</c> when the time since
        /// last update exceeds <see cref="UpdateFrequency"/>.
        /// </remarks>
        protected virtual bool ShouldUpdateTransform()
        {
            return (Time.unscaledTime - lastUpdateTime) >= updateFrequency;
        }
        #endregion // Overridables / Event Triggers

        #region Overridables / Event Triggers
        /// <summary>
        /// Returns a value that indicates if the parent should be considered
        /// valid in its current state.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the parent is valid; otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The default implementation checks to make sure the
        /// parent is meeting minimum requirements by calling
        /// <see cref="ParentAlignmentOptions.IsMeetingRequirements"/>.
        /// </remarks>
        protected virtual bool IsValidParent(ParentAlignmentOptions parent)
        {
            // Meeting minimum requirements?
            return parent.IsMeetingRequirements();
        }

        /// <summary>
        /// Called when the value of the <see cref="CurrentParents"/> property has changed.
        /// </summary>
        protected virtual void OnCurrentParentsChanged()
        {
            CurrentParentsChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion // Overridables / Event Triggers

        #region Unity Overrides
        /// <inheritdoc />
        protected override void OnEnable()
        {
            // Pass to base first
            base.OnEnable();

            // Perform immediate update
            UpdateTransform(force: true);
        }

        /// <inheritdoc />
        protected override void Update()
        {
            // Call base first
            base.Update();

            // Should we update the transform?
            if (ShouldUpdateTransform())
            {
                // Yes, update
                UpdateTransform();

                // Store last update time
                lastUpdateTime = Time.unscaledTime;
            }
        }
        #endregion // Unity Overrides

        #region Public Methods
        /// <summary>
        /// Attempts to calculate and update the transform.
        /// </summary>
        /// <param name="force">
        /// <c>true</c> to force an update even if the same parent option is
        /// selected. The default is <c>false</c>.
        /// </param>
        public virtual void UpdateTransform(bool force=false)
        {
            // If there are no parent options, nothing to do
            if (parentOptions.Count == 0)
            {
                State = AlignmentState.Inhibited;
                Debug.LogWarning($"{nameof(MultiParentAlignment)}: No parent options to select from.");
                return;
            }

            // Validate the parent options
            ValidateParentOptions();

            // Use virtual method to select the best parent option
            CurrentParents = SelectParents();

            // Actually apply the parents
            ApplyParents(force: force);

            // Resolved
            State = AlignmentState.Tracking;
        }

        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets the list of currently selected <see cref="ParentAlignmentOptions"/>.
        /// </summary>
        public List<ParentAlignmentOptions> CurrentParents
        {
            get
            {
                return currentParents;
            }
            protected set
            {
                if (currentParents != value)
                {
                    currentParents = value;
                    OnCurrentParentsChanged();
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

        /// <summary>
        /// Gets or sets the list of parent alignment options.
        /// </summary>
        /// <remarks>
        /// Replacing this list will cause the transform to be recalculated.
        /// </remarks>
        public List<ParentAlignmentOptions> ParentOptions
        {
            get
            {
                return parentOptions;
            }
            set
            {
                // Ensure changing
                if (parentOptions != value)
                {
                    // Validate
                    if (value == null) throw new ArgumentNullException(nameof(value));

                    // Store
                    parentOptions = value;

                    // Attempt to update the transform
                    UpdateTransform();
                }
            }
        }

        /// <summary>
        /// Gets or sets the transform that will serve as the frame of reference.
        /// </summary>
        /// <remarks>
        /// This transform is used when calculating modes like
        /// <see cref="MultiParentAlignmentMode.NearestNeighbor">NearestNeighbor</see>.
        /// By default, if this property is <see langword = "null" /> the main camera
        /// transform will be used.
        /// </remarks>
        public Transform ReferenceTransform
        {
            get
            {
                return referenceTransform;
            }
            set
            {
                // Ensure changing
                if (referenceTransform != value)
                {
                    // Store
                    referenceTransform = value;

                    // Attempt to update the transform
                    UpdateTransform();
                }
            }
        }

        /// <summary>
        /// Gets or sets the time between updates (in seconds).
        /// </summary>
        /// <remarks>
        /// By default, if this value is zero alignment will be updated on every frame.
        /// </remarks>
        public float UpdateFrequency
        {
            get
            {
                return updateFrequency;
            }
            set
            {
                // Validate
                if (value < 0.0f) throw new ArgumentOutOfRangeException(nameof(value));

                // Store
                updateFrequency = value;
            }
        }
        #endregion // Public Properties

        #region Public Events
        /// <summary>
        /// Raised when the value of the <see cref="CurrentParents"/> property has changed.
        /// </summary>
        public event EventHandler CurrentParentsChanged;
        #endregion // Public Events
    }
}