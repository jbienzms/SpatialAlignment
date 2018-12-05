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
        #endregion // Unity Inspector Variables

        #region Overrides / Event Handlers
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

        /// <summary>
        /// Restores the last transform to the current transform.
        /// </summary>
        protected virtual void RestoreLastTransform()
        {
            transform.position = lastPosition;
            transform.rotation = lastRotation;
        }

        /// <summary>
        /// Saves the current transform as the last transform.
        /// </summary>
        protected virtual void SaveLastTransform()
        {
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
        #endregion // Overrides / Event Handlers

        #region Unity Overrides
        // Start is called before the first frame update
        protected virtual void Start()
        {
            if (refineOnStart)
            {
                StartRefinement();
            }
        }

        // Update is called once per frame
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