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
using UnityEngine;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// Defines the directions that can be used in a refinement.
    /// </summary>
    public enum RefinementDirection
    {
        /// <summary>
        /// Forward.
        /// </summary>
        Forward,

        /// <summary>
        /// Back.
        /// </summary>
        Back,

        /// <summary>
        /// Down.
        /// </summary>
        Down,

        /// <summary>
        /// Left.
        /// </summary>
        Left,

        /// <summary>
        /// Right
        /// </summary>
        Right,

        /// <summary>
        /// Up.
        /// </summary>
        Up,
    }

    /// <summary>
    /// The base class for a behavior that provides cancelable refinement of the
    /// transform of an object.
    /// </summary>
    public class RefinementBase : MonoBehaviour
    {
        #region Member Variables
        private bool isRefining;
        private Vector3 lastPosition;
        private Quaternion lastRotation;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("Whether to begin refining when the behavior starts.")]
        private bool refineOnStart;

        [SerializeField]
        [Tooltip("The coordinate system to use when performing operations.")]
        private Space space = Space.Self;

        [SerializeField]
        [Tooltip("Optional transform where nudge operations will be applied. If none is specified, the transform of the applied GameObject will be used.")]
        private Transform targetTransform;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Gets a relative "look" direction for the <see cref="TargetTransform"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="RefinementDirection"/> that represents the look
        /// direction for the target transform.
        /// </returns>
        protected virtual RefinementDirection GetLookDirection()
        {
            // Which "forward" are we using
            Vector3 forward;
            if (space == Space.World)
            {
                // Just use controller forward
                forward = transform.forward;
            }
            else
            {
                // Use controller forward but in target local space
                forward = targetTransform.InverseTransformDirection(transform.forward);
            }

            // Get the absolute axis for the forward direction (snaps to axis only)
            forward = forward.AbsoluteAxis();

            // Normalize it toward 1.0
            forward = forward.normalized;

            // Try to convert the vector to a direction
            RefinementDirection direction;
            if (forward.TryGetDirection(out direction))
            {
                return direction;
            }
            else
            {
                return RefinementDirection.Forward;
            }
        }

        /// <summary>
        /// Restores the last transform to the current transform.
        /// </summary>
        protected virtual void RestoreLastTransform()
        {
            targetTransform.position = lastPosition;
            targetTransform.rotation = lastRotation;
        }

        /// <summary>
        /// Saves the current transform as the last transform.
        /// </summary>
        protected virtual void SaveLastTransform()
        {
            lastPosition = targetTransform.position;
            lastRotation = targetTransform.rotation;
        }
        #endregion // Internal Methods

        #region Overridables / Event Triggers
        /// <summary>
        /// Called when the user has canceled refinement.
        /// </summary>
        protected virtual void OnRefinementCanceled()
        {
            RefinementCanceled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the user has requested cancellation of the refinement.
        /// </summary>
        protected virtual void OnRefinementCanceling()
        {
            RefinementCanceling?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the user has finished refinement.
        /// </summary>
        protected virtual void OnRefinementFinished()
        {
            RefinementFinished?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the user has started refinement.
        /// </summary>
        protected virtual void OnRefinementStarted()
        {
            RefinementStarted?.Invoke(this, EventArgs.Empty);
        }
        #endregion // Overridables / Event Triggers

        #region Unity Overrides
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        protected virtual void Awake()
        {
            // If no transform is specified, use the GameObjects transform
            if (targetTransform == null)
            {
                targetTransform = gameObject.transform;
            }
        }

        /// <summary>
        /// This function is called after all frame updates for the last frame of the object’s existence.
        /// </summary>
        protected virtual void OnDestroy()
        {

        }

        /// <summary>
        /// This function is called when the behavior becomes disabled or inactive.
        /// </summary>
        protected virtual void OnDisable()
        {

        }

        /// <summary>
        /// Only called if the Object is active, this function is called just after the object is enabled.
        /// </summary>
        protected virtual void OnEnable()
        {

        }

        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        protected virtual void Start()
        {
            if (refineOnStart)
            {
                StartRefinement();
            }
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        protected virtual void Update()
        {

        }
        #endregion // Unity Overrides

        #region Public Methods
        /// <summary>
        /// Cancels refinement and restores the original transform.
        /// </summary>
        public void CancelRefinement()
        {
            // Make sure we're refining
            if (!isRefining)
            {
                Debug.LogWarning($"{nameof(CancelRefinement)} called but not refining.");
                return;
            }

            // Call override
            OnRefinementCanceling();

            // Not refining
            isRefining = false;

            // Restore transform since canceled
            RestoreLastTransform();

            // Call override
            OnRefinementCanceled();
        }

        /// <summary>
        /// Finishes refinement.
        /// </summary>
        public void FinishRefinement()
        {
            // Make sure we're refining
            if (!isRefining)
            {
                Debug.LogWarning($"{nameof(FinishRefinement)} called but not refining.");
                return;
            }

            // Not refining
            isRefining = false;

            // Call override
            OnRefinementFinished();
        }

        /// <summary>
        /// Starts refinement.
        /// </summary>
        public void StartRefinement()
        {
            // Make sure we're not already refining
            if (isRefining)
            {
                Debug.LogWarning($"{nameof(StartRefinement)} called but already refining.");
                return;
            }

            // Save transform in case of cancellation
            SaveLastTransform();

            // Call override
            OnRefinementStarted();

            // Refining
            isRefining = true;
        }
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets a value that indicates if the controller is currently refining
        /// the transform.
        /// </summary>
        public bool IsRefining { get => isRefining; }

        /// <summary>
        /// Gets or sets whether to begin refining when the behavior starts.
        /// </summary>
        public bool RefineOnStart { get => refineOnStart; set => refineOnStart = value; }

        /// <summary>
        /// Gets or sets the coordinate system to use when performing operations.
        /// </summary>
        /// <remarks>
        /// The default is <see cref="Space.Self"/>.
        /// </remarks>
        public Space Space { get { return space; } set { space = value; } }

        /// <summary>
        /// Gets or sets an optional transform where nudge operations will be applied. If none is specified, the transform of the applied GameObject will be used.
        /// </summary>
        public Transform TargetTransform { get { return targetTransform; } set { targetTransform = value; } }
        #endregion // Public Properties

        #region Public Events
        /// <summary>
        /// Raised when the user has canceled refinement.
        /// </summary>
        public event EventHandler RefinementCanceled;

        /// <summary>
        /// Raised when the user has requested cancellation of the refinement.
        /// </summary>
        public event EventHandler RefinementCanceling;

        /// <summary>
        /// Raised when the user has finished refinement.
        /// </summary>
        public event EventHandler RefinementFinished;

        /// <summary>
        /// Raised when the user has started refinement.
        /// </summary>
        public event EventHandler RefinementStarted;
        #endregion // Public Events
    }
}