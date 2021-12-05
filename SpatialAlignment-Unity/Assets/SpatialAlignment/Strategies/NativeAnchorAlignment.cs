#if UNITY_WSA && !UNITY_2020_1_OR_NEWER
#define SAA_LEGACY_XR
#endif
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

#if SAA_LEGACY_XR

using Microsoft.SpatialAlignment.Persistence;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using NativeAnchor = UnityEngine.XR.WSA.WorldAnchor;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// An alignment strategy that attaches the object to a platform-specific native world anchor.
    /// </summary>
    /// <remarks>
    /// On HoloLens the native anchor is <see cref="UnityEngine.XR.WSA.WorldAnchor">WorldAnchor</see>.
    /// </remarks>
    [DataContract]
    public class NativeAnchorAlignment : AlignmentStrategy, INativePersistence
    {
        #region Member Variables
        private NativeAnchor nativeAnchor;
        private WorldAnchorStore anchorStore;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Tooltip("The ID of the anchor to load.")]
        private string anchorId;

        [DataMember]
        [SerializeField]
        [Tooltip("Whether the anchor should be loaded when the behavior starts.")]
        private bool loadOnStart = false;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Ensures that the application has access to the anchor store.
        /// </summary>
        /// <returns>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
#pragma warning disable CS1998 // Await is compiled out for non-WSA builds.
        private async Task EnsureAnchorStoreAsync()
#pragma warning restore CS1998
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
                throw new UnauthorizedAccessException($"{nameof(NativeAnchorAlignment)}: WorldAnchorStore could not be acquired.");
            }
        }

        /// <summary>
        /// Subscribe to anchor events.
        /// </summary>
        private void SubscribeAnchor()
        {
            if (Application.isEditor)
            {
                // The anchor store will never be available in the editor, so
                // when in the editor just pretend that the anchor is resolved
                State = AlignmentState.Resolved;
            }
            else
            {
                // Subscribe to anchor events
                nativeAnchor.OnTrackingChanged += Anchor_OnTrackingChanged;

                // Update state based on anchor state
                State = (nativeAnchor.isLocated ? AlignmentState.Tracking : AlignmentState.Unresolved);
            }
        }

        /// <summary>
        /// Unsubscribe from anchor events.
        /// </summary>
        private void UnsubscribeAnchor()
        {
            // Unsubscribe from events
            nativeAnchor.OnTrackingChanged -= Anchor_OnTrackingChanged;
        }
        #endregion // Internal Methods

        #region INativePersistence Members
        Task INativePersistence.LoadNativeAsync()
        {
            return TryLoadNativeAsync();
        }

        Task INativePersistence.SaveNativeAsync()
        {
            return SaveNativeAsync();
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
        protected override void OnDisable()
        {
            // Make sure the anchor is unloaded
            UnloadNative();

            // Pass on to base
            base.OnDisable();
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            // Pass to base first
            base.OnEnable();

            // Load or create new?
            if (loadOnStart)
            {
                // Try to load
                var t = TryLoadNativeAsync();
            }
            else
            {
                // Create a new one
                CreateNative();
            }
        }
        #endregion // Unity Overrides

        #region Public Methods
        /// <summary>
        /// Attempts to find a native anchor already on the same GameObject.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the native anchor was found; otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="CreateNative"/>
        /// <seealso cref="FindOrCreateNative"/>
        public bool FindNative()
        {
            // Already found?
            if (nativeAnchor != null)
            {
                return true;
            }

            // Try to find it
            nativeAnchor = gameObject.GetComponent<NativeAnchor>();

            // Found?
            return nativeAnchor != null;
        }

        /// <summary>
        /// Attempts to find a native anchor already on the same
        /// GameObject and creates one if none is found.
        /// </summary>
        /// <seealso cref="CreateNative"/>
        /// <seealso cref="FindNative"/>
        public void FindOrCreateNative()
        {
            if (!FindNative())
            {
                // Add the native anchor
                nativeAnchor = gameObject.AddComponent<NativeAnchor>();

                // Subscribe to events
                SubscribeAnchor();
            }
        }

        /// <summary>
        /// Creates a new native anchor. If one already exists, it is replaced.
        /// </summary>
        /// <seealso cref="FindNative"/>
        /// <seealso cref="FindOrCreateNative"/>
        public void CreateNative()
        {
            // Unload any existing
            if (nativeAnchor != null)
            {
                UnloadNative();
            }

            // Use FindOrCreate to Create a new one
            FindOrCreateNative();
        }

        /// <summary>
        /// Attempts to load the native anchor specified by <see cref="AnchorId"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
#pragma warning disable CS1998 // Await is compiled out for UNITY_EDITOR builds.
        public async Task<bool> TryLoadNativeAsync()
#pragma warning restore CS1998
        {
            // Validate the ID
            if (string.IsNullOrEmpty(anchorId))
            {
                State = AlignmentState.Error;
                Debug.LogError($"{nameof(NativeAnchorAlignment)}: {nameof(AnchorId)} is not valid.");
                return false;
            }

            // If there was an existing anchor, unload it
            if (nativeAnchor != null) { UnloadNative(); }

            // The anchor store is not accessible in the editor
            if (!Application.isEditor)
            {
                // Make sure we have access to the anchor store
                await EnsureAnchorStoreAsync();

                // Now try to load the anchor itself
                nativeAnchor = anchorStore.Load(anchorId, this.gameObject);
            }

            // If still not loaded, log and fail
            if (nativeAnchor == null)
            {
                State = AlignmentState.Error;
                Debug.LogError($"{nameof(NativeAnchorAlignment)}: {nameof(NativeAnchor)} with the id '{anchorId}' was not found.");
                return false;
            }
            else
            {
                // Subscribe
                SubscribeAnchor();

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
#pragma warning disable CS1998 // Await is compiled out for UNITY_EDITOR builds.
        public async Task SaveNativeAsync()
#pragma warning restore CS1998
        {
            // Validate the ID
            if (string.IsNullOrEmpty(anchorId))
            {
                throw new InvalidOperationException($"{nameof(NativeAnchorAlignment)}: {nameof(AnchorId)} is not valid.");
            }

            // If there is no anchor, create it
            if (nativeAnchor == null) { CreateNative(); }

            // The anchor store is not accessible in the editor
            if (!Application.isEditor)
            {
                // Make sure we have access to the anchor store
                await EnsureAnchorStoreAsync();

                // Now try to save the anchor itself
                anchorStore.Save(anchorId, nativeAnchor);
            }
        }

        /// <summary>
        /// Unloads any attached native anchor and stops tracking.
        /// </summary>
        public void UnloadNative()
        {
            // Make sure we have an anchor to unload
            if (nativeAnchor == null) { return; }

            // Unsubscribe from anchor events
            UnsubscribeAnchor();

            // Destroy the anchor (which also removes it from the game object)
            Destroy(nativeAnchor);

            // Reset the reference
            nativeAnchor = null;

            // No longer tracking
            State = AlignmentState.Unresolved;
        }
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets or sets the ID of the anchor to load.
        /// </summary>
        public string AnchorId { get { return anchorId; } set { anchorId = value; } }

        /// <summary>
        /// Gets or sets a value that indicates if the anchor should be loaded when the behavior starts.
        /// </summary>
        public bool LoadOnStart { get { return loadOnStart; } set { loadOnStart = value; } }

        /// <summary>
        /// Gets the underlying native anchor.
        /// </summary>
        public NativeAnchor NativeAnchor { get { return nativeAnchor; } }
        #endregion // Public Properties
    }
}
#endif // SAA_LEGACY_XR
