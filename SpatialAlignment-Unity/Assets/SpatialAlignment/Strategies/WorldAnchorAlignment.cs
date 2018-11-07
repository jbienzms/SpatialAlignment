﻿//
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

using Microsoft.SpatialAlignment.Persistence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// An alignment strategy that attaches the object to a HoloLens world anchor.
    /// </summary>
    public class WorldAnchorAlignment : AlignmentStrategy, INativePersistence
    {
        #region Member Variables
        private WorldAnchor anchor;
        private WorldAnchorStore anchorStore;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The ID of the anchor to load.")]
        private string anchorId;

        [SerializeField]
        [Tooltip("Whether the anchor should be loaded when the behavior starts.")]
        private bool loadOnStart = true;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Ensures that the application has access to the anchor store.
        /// </summary>
        /// <returns>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        private async Task EnsureAnchorStoreAsync()
        {
            // Do we need to acquire the store?
            if (anchorStore == null)
            {
                // Create the completion source
                TaskCompletionSource<bool> tc = new TaskCompletionSource<bool>();

                // Start process to get store via callback
                WorldAnchorStore.GetAsync((WorldAnchorStore store) =>
                {
                    if (store != null) { anchorStore = store; }
                    tc.SetResult(store != null);
                });

                // Wait for callback to complete
                await tc.Task;
            }

            // If store is still not acquired, log and fail
            if (anchorStore == null)
            {
                State = AlignmentState.Error;
                throw new UnauthorizedAccessException($"{nameof(WorldAnchorAlignment)}: {nameof(WorldAnchorStore)} could not be acquired.");
            }
        }
        #endregion // Internal Methods

        #region INativePersistence Members
        Task INativePersistence.LoadNativeAsync()
        {
            return TryLoadAnchorAsync();
        }

        Task INativePersistence.SaveNativeAsync()
        {
            return SaveAnchorAsync();
        }
        #endregion // INativePersistence Members

        #region Overrides / Event Handlers
        private void Anchor_OnTrackingChanged(WorldAnchor worldAnchor, bool located)
        {
            // Update state based on anchor tracking state
            State = (located ? AlignmentState.Tracking : AlignmentState.Unresolved);
        }
        #endregion // Overrides / Event Handlers

        #region Unity Overrides
        /// <inheritdoc />
        protected virtual void OnDestroy()
        {
            UnloadAnchor();
        }

        /// <inheritdoc />
        protected override void Start()
        {
            // Call base first
            base.Start();

            // Load?
            if (loadOnStart)
            {
                var t = TryLoadAnchorAsync();
            }
        }
        #endregion // Unity Overrides

        #region Public Methods
        /// <summary>
        /// Attempts to load the native anchor specified by <see cref="AnchorId"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public async Task<bool> TryLoadAnchorAsync()
        {
            // If the anchor is already loaded, we're good
            if (anchor != null) { return true; }

            // Validate the ID
            if (string.IsNullOrEmpty(anchorId))
            {
                State = AlignmentState.Error;
                throw new InvalidOperationException($"{nameof(WorldAnchorAlignment)}: {nameof(AnchorId)} is not valid.");
            }

            // Make sure we have access to the anchor store
            await EnsureAnchorStoreAsync();

            // Now try to load the anchor itself
            anchor = anchorStore.Load(anchorId, this.gameObject);

            // If still not loaded, log and fail
            if (anchor == null)
            {
                State = AlignmentState.Error;
                Debug.LogError($"{nameof(WorldAnchorAlignment)}: {nameof(WorldAnchor)} with the id '{anchorId}' was not found.");
                return false;
            }
            else
            {
                // Subscribe to anchor events
                anchor.OnTrackingChanged += Anchor_OnTrackingChanged;

                // Update state based on anchor state
                State = (anchor.isLocated ? AlignmentState.Tracking : AlignmentState.Unresolved);

                // Loaded!
                return true;
            }
        }

        /// <summary>
        /// Attempts to save the native anchor with the specified <see cref="AnchorId"/>.
        /// If the current game object doesn't have an anchor, it will automatically
        /// be created.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public async Task SaveAnchorAsync()
        {
            // Make sure the anchor is created
            if (anchor == null)
            {
                anchor = this.gameObject.AddComponent<WorldAnchor>();
            }

            // Validate the ID
            if (string.IsNullOrEmpty(anchorId))
            {
                throw new InvalidOperationException($"{nameof(WorldAnchorAlignment)}: {nameof(AnchorId)} is not valid.");
            }

            // Make sure we have access to the anchor store
            await EnsureAnchorStoreAsync();

            // Now try to save the anchor itself
            anchorStore.Save(anchorId, anchor);
        }

        /// <summary>
        /// Unloads any attached <see cref="WorldAnchor"/> and stops tracking.
        /// </summary>
        public void UnloadAnchor()
        {
            // Make sure we have an anchor to unload
            if (anchor == null) { return; }

            // Unsubscribe from events
            anchor.OnTrackingChanged -= Anchor_OnTrackingChanged;

            // Destroy the anchor (which also removes it from the game object)
            DestroyImmediate(anchor);

            // Reset the reference
            anchor = null;

            // No longer tracking
            State = AlignmentState.Unresolved;
        }
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets or sets the ID of the anchor to load.
        /// </summary>
        public string AnchorId { get => anchorId; set => anchorId = value; }

        /// <summary>
        /// Gets or sets a value that indicates if the anchor should be loaded when the behavior starts.
        /// </summary>
        public bool LoadOnStart { get => loadOnStart; set => loadOnStart = value; }
        #endregion // Public Properties
    }
}