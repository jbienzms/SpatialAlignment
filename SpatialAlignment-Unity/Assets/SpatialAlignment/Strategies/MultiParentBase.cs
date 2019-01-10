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
    /// A base class for a strategy that coordinates alignment based on the alignment
    /// of multiple "parent" objects.
    /// </summary>
    [DataContract]
    public abstract class MultiParentBase : AlignmentStrategy
    {
        #region Member Variables
        private List<ParentAlignmentOptions> currentParents;
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

        #region Overridables / Event Triggers
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
        protected abstract void ApplyParents(bool force = false);

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
        protected abstract List<ParentAlignmentOptions> SelectParents();

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

        /// <summary>
        /// Validates the contents of the <see cref="ParentOptions"/> collection.
        /// </summary>
        /// <remarks>
        /// The default implementation simply validates that all parent options have a valid parent.
        /// </remarks>
        protected virtual void ValidateParents()
        {
            // Validate each option
            for (int i = 0; i < parentOptions.Count; i++)
            {
                var opt = parentOptions[i];
                if (opt.Frame == null) { throw new InvalidOperationException($"{nameof(ParentAlignmentOptions)}.{nameof(ParentAlignmentOptions.Frame)} can't be null."); }
                if (opt.Frame.transform == null) { throw new InvalidOperationException($"{nameof(SpatialFrame)}.{nameof(SpatialFrame.transform)} can't be null."); }
            }
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
            ValidateParents();

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