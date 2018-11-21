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
        #region Member Variables
        private ParentAlignmentOptions currentParent;
        private float lastUpdateTime;
        #endregion // Member Variables

        #region Unity Inspector Variables
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
        /// Applies the specified parent options to this object.
        /// </summary>
        /// <param name="parentOption">
        /// The parent options to apply.
        /// </param>
        /// <param name="force">
        /// <c>true</c> to force an update even if reusing the same option.
        /// The default is <c>false</c>.
        /// </param>
        /// <remarks>
        /// The default implementation of this method parents the transform
        /// and applies position, rotation and scale modifications.
        /// </remarks>
        protected virtual void ApplyParent(ParentAlignmentOptions parentOption, bool force=false)
        {
            // Validate
            if (parentOption == null) { throw new ArgumentNullException(nameof(parentOption)); }
            if (parentOption.Frame == null) { throw new InvalidOperationException($"{nameof(parentOption.Frame)} cannot be null."); }

            // If already parented to this object, no additional work needed
            if ((!force) && (currentParent == parentOption)) { return; }

            // Make our transform a child of the frame
            this.transform.SetParent(parentOption.Frame.transform, worldPositionStays: false);

            // Apply transform modifications
            this.transform.localPosition = parentOption.Position;
            this.transform.localRotation = Quaternion.Euler(parentOption.Rotation);
            this.transform.localScale = parentOption.Scale;

            // Notify of parent change
            CurrentParent = parentOption;

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
        /// Attempts to select the best parent option based on current settings.
        /// </summary>
        /// <returns>
        /// The <see cref="ParentAlignmentOptions"/> that represent the best parent,
        /// if found; otherwise <see langword = "null" />.
        /// </returns>
        /// <remarks>
        /// The default implementation examines all options where
        /// <see cref="ParentAlignmentOptions.IsValidTarget">IsValidTarget</see>
        /// returns <c>true</c> and selects the top option sorted by
        /// <see cref="ParentAlignmentOptions.SortOrder(Transform)">SortOrder</see>.
        /// </remarks>
        protected virtual ParentAlignmentOptions SelectParent()
        {
            // Attempt to get the reference transform
            Transform reference = GetReferenceTransform();

            // If no transform, can't continue
            if (reference == null) { return null; }

            // Placeholder
            ParentAlignmentOptions parentOption = null;

            // If only one parent option, always use that
            if (parentOptions.Count == 1)
            {
                // Use only parent option, if valid
                parentOption = (parentOptions[0].IsValidTarget() ? parentOptions[0] : null);
            }
            else
            {
                // Find the best valid parent, sorted by logic in the ParentAlignmentOption itself
                parentOption = (from o in ParentOptions
                                where o.IsValidTarget()
                                orderby o.SortOrder(reference)
                                select o
                               ).FirstOrDefault();

            }

            // Done searching
            return parentOption;
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
        /// Called when the value of the <see cref="CurrentParent"/> property has changed.
        /// </summary>
        protected virtual void OnCurrentParentChanged()
        {
            CurrentParentChanged?.Invoke(this, EventArgs.Empty);
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
            ParentAlignmentOptions parentOption = SelectParent();

            // If no option could be found, fail
            if (parentOption == null)
            {
                State = AlignmentState.Inhibited;
                Debug.LogWarning($"{nameof(MultiParentAlignment)}: No parent could be found that meets the minimum criteria.");
                return;
            }
            else
            {
                // Actually apply the parent
                ApplyParent(parentOption, force: force);

                // Resolved
                State = AlignmentState.Tracking;
            }
        }

        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets the <see cref="ParentAlignmentOptions"/> that represent the currently
        /// selected parent.
        /// </summary>
        public virtual ParentAlignmentOptions CurrentParent
        {
            get
            {
                return currentParent;
            }
            protected set
            {
                if (currentParent != value)
                {
                    currentParent = value;
                    OnCurrentParentChanged();
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
        /// <inheritdoc />
        public event EventHandler CurrentParentChanged;
        #endregion // Public Events
    }
}