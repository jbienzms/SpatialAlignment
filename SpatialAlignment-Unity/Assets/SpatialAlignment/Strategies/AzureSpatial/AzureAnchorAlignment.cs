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

using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity.Samples;
using Microsoft.SpatialAlignment.Persistence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;

namespace Microsoft.SpatialAlignment.Azure
{
    /// <summary>
    /// An alignment strategy that attaches the object to an Azure Spatial Anchor.
    /// </summary>
    [DataContract]
    public class AzureAnchorAlignment : NativeAnchorAlignment
    {
        #region Member Variables
        private CloudSpatialAnchor cloudAnchor;
        private AzureSpatialAnchorsDemoWrapper cloudManager;
        #endregion // Member Variables

        #region Internal Methods
        /// <summary>
        /// Stores the specified cloud version of the anchor and creates or updates the native anchor
        /// to match.
        /// </summary>
        /// <param name="anchor">
        /// The cloud version of the anchor.
        /// </param>
        /// <remarks>
        /// When this method completes, <see cref="CloudAnchor"/> will point to the anchor specified
        /// by <paramref name="anchor"/> and <see cref="NativeAnchor"/> will return a new or updated
        /// native anchor with the same information.
        /// </remarks>
        private void CloudToNative(CloudSpatialAnchor anchor)
        {
            // Validate
            if (anchor == null) throw new ArgumentNullException(nameof(anchor));

            #if UNITY_IOS

            // Remove any existing ARKit native anchor if found
            NativeAnchor oldAnchor = gameObject.GetComponent<NativeAnchor>();
            if (oldAnchor != null)
            {
                DestroyImmediate(oldAnchor);
            }

            // Get ARKit anchor from cloud-based native anchor
            UnityARUserAnchorData anchorData = UnityARSessionNativeInterface.UnityAnchorDataFromArkitAnchorPtr(anchor.LocalAnchor);

            // Do matrix conversion
            Matrix4x4 matrix4X4 = GetMatrix4x4FromUnityAr4x4(anchorData.transform);

            // Move object to anchor position
            gameObject.transform.position = UnityARMatrixOps.GetPosition(matrix4X4);
            gameObject.transform.rotation = UnityARMatrixOps.GetRotation(matrix4X4);

            // Add ARKit native anchor
            nativeAnchor = gameObject.AddComponent<NativeAnchor>();

            #elif WINDOWS_UWP

            // Use existing World Anchor if found, otherwise create a new one
            FindOrCreateNative();

            // Update the World Anchor to use the cloud-based native anchor
            nativeAnchor.SetNativeSpatialAnchorPtr(anchor.LocalAnchor);

            #endif
        }

        /// <summary>
        /// Ensures that the application has a valid cloud manager instance.
        /// </summary>
        private void EnsureManager()
        {
            // Do we need to acquire the store?
            if (cloudManager == null)
            {
                cloudManager = AzureSpatialAnchorsDemoWrapper.Instance;
            }

            // If store is still not acquired, log and fail
            if (cloudManager == null)
            {
                State = AlignmentState.Error;
                throw new InvalidOperationException($"{nameof(AzureAnchorAlignment)}: {nameof(AzureSpatialAnchorsDemoWrapper)} was not found.");
            }
        }

        /// <summary>
        /// Creates or updates the <see cref="CloudSpatialAnchor"/> returned by
        /// <see cref="CloudAnchor"/> to reflect the same data as the native anchor.
        /// </summary>
        /// <remarks>
        /// If no native anchor exists on the game object it will be created.
        /// </remarks>
        private async Task NativeToCloudAsync()
        {
            // Make sure there's a native anchor
            FindOrCreateNative();

            // Make sure the cloud anchor is created
            if (cloudAnchor == null)
            {
                cloudAnchor = new CloudSpatialAnchor();
            }

            // Update the cloud native anchor pointer
            #if UNITY_IOS

            Debug.Log($"##### About to wait for anchor. Cloud Anchor {cloudAnchor}");
            Debug.Log($"##### About to wait for anchor. Native Anchor {nativeAnchor}");
            Debug.Log($"##### About to wait for anchor. Native Anchor ID {nativeAnchor.AnchorId}");

            // HACK: Wait for ARKit to assign an anchor ID
            IntPtr nativeId = arkitSession.GetArAnchorPointerForId(nativeAnchor.AnchorId);
            while (nativeId == IntPtr.Zero)
            {
                await Task.Delay(10);
                nativeId = arkitSession.GetArAnchorPointerForId(nativeAnchor.AnchorId);
            }

            Debug.Log($"##### Done waiting for anchor. Native Anchor ID {nativeId}");
            Debug.Log($"##### Done waiting for anchor. ARKit Session {arkitSession}");

            cloudAnchor.LocalAnchor = nativeId;

            #elif WINDOWS_UWP

            cloudAnchor.LocalAnchor = nativeAnchor.GetNativeSpatialAnchorPtr();

            #endif

            // Verify we got a native anchor pointer
            if (cloudAnchor.LocalAnchor == IntPtr.Zero)
            {
                throw new InvalidOperationException("Couldn't obtain a native anchor pointer");
            }
        }

        /// <summary>
        /// Subscribe to cloud manager events.
        /// </summary>
        private void SubscribeManager()
        {
            cloudManager.OnAnchorLocated += CloudManager_OnAnchorLocated;
        }

        /// <summary>
        /// Unsubscribe from cloud manager events.
        /// </summary>
        private void UnsubscribeManager()
        {
            cloudManager.OnAnchorLocated -= CloudManager_OnAnchorLocated;
        }

        #endregion // Internal Methods

        #region Overrides / Event Handlers
        private void CloudManager_OnAnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            // Is it our anchor?
            if (args.Identifier == AnchorId)
            {
                // Was the anchor just located?
                // TODO: Do we also need to track LocateAnchorStatus.AlreadyTracked?
                if (args.Status == LocateAnchorStatus.Located)
                {
                    // Sync cloud anchor data to native anchor data but do it
                    // using the Unity main thread since behaviors may need to
                    // be created or updated.
                    UnityDispatcher.InvokeOnAppThread(()=> CloudToNative(args.Anchor));
                }
            }
        }
        #endregion // Overrides / Event Handlers

        #region Unity Overrides
        /// <inheritdoc />
        protected override void OnDisable()
        {
            // If we have a manager, unsubscribe
            if (cloudManager != null) { UnsubscribeManager(); }

            // Pass on to base
            base.OnDisable();
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            // Make sure we've got a manager
            EnsureManager();

            // Subscribe to manager events
            SubscribeManager();

            // Pass on to base to finish
            base.OnEnable();
        }

        protected override Task OnLoadAsync()
        {
            // Do NOT call base because we don't actually want to load the
            // underlying native anchor.
            return TryLoadCloudAsync();
        }

        protected override Task OnSaveAsync()
        {
            // Do NOT call base because we don't actually want to save the
            // underlying native anchor.
            return SaveCloudAsync();
        }
        #endregion // Unity Overrides

        #region Public Methods
        /// <summary>
        /// Attempts to load the cloud anchor specified by <see cref="AnchorId"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public async Task<bool> TryLoadCloudAsync()
        {
            // Validate the ID
            if (string.IsNullOrEmpty(AnchorId))
            {
                State = AlignmentState.Error;
                Debug.LogError($"{nameof(AzureAnchorAlignment)}: {nameof(AnchorId)} is not valid.");
                return false;
            }

            // The cloud manager is not accessible in the editor
            #if !UNITY_EDITOR

            // Make sure we have access to the cloud manager
            await EnsureManagerAsync();

            // Now try to load the anchor itself
            List<string> anchorsToFind = new List<string>();
            anchorsToFind.Add(anchorId);
            cloudManager.SetAnchorIdsToLocate(ref anchorsToFind);
            // TODO: ^ Support more than one above ^

            #endif // !UNITY_EDITOR

            // If still not loaded, log and fail
            if (cloudAnchor == null)
            {
                State = AlignmentState.Error;
                Debug.LogError($"{nameof(AzureAnchorAlignment)}: {nameof(CloudSpatialAnchor)} with the id '{AnchorId}' was not found.");
                return false;
            }
            else
            {
                // Apply cloud to native
                CloudToNative(cloudAnchor);

                // Loaded!
                return true;
            }
        }

        /// <summary>
        /// Attempts to save the cloud anchor with the specified <see cref="AnchorId"/>.
        /// If the current game object doesn't have an anchor, it will automatically
        /// be created.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public async Task SaveCloudAsync()
        {
            // Validate the ID
            if (string.IsNullOrEmpty(AnchorId))
            {
                throw new InvalidOperationException($"{nameof(AzureAnchorAlignment)}: {nameof(AnchorId)} is not valid.");
            }

            // If there is no anchor, create it
            if (cloudAnchor == null) { cloudAnchor = new CloudSpatialAnchor(); }

            // The anchor store is not accessible in the editor
            #if !UNITY_EDITOR

            // Make sure we have the cloud manager
            await EnsureManagerAsync();

            // Convert native anchor format to cloud anchor format
            await NativeToCloudAsync();

            // Now try to save cloud anchor
            var savedAnchor = await cloudManager.StoreAnchorInCloud(anchor.CloudAnchor.LocalAnchor);

            // Make sure save was successful
            if (savedAnchor == null)
            {
                State = AlignmentState.Error;
                Debug.LogError($"{nameof(CloudSpatialAnchor)} could not be saved.");
                return;
            }

            // Update our anchor ID to point at cloud ID (which may have just
            // been generated).
            AnchorId = savedAnchor.Identifier;

            // Sync saved cloud state back into native state
            anchor.CloudToNative(savedAnchor);

            #endif // !UNITY_EDITOR
        }

        /// <summary>
        /// Sets an existing anchor as the cloud anchor for this alignment.
        /// </summary>
        /// <param name="anchor">
        /// The existing <see cref="CloudSpatialAnchor"/> to set.
        /// </param>
        /// <remarks>
        /// NOTE: This capability exists as a standalone method rather than a
        /// setter on the <see cref="CloudAnchor"/> property because calling it
        /// results in significant and observable side effects. The side
        /// effects may include the removal of and recreation of the underlying
        /// native anchor. For more information see
        /// <see href="https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/ms229054(v=vs.100)">
        /// Choosing Between Properties and Methods</see>.
        /// </remarks>
        public void SetCloudAnchor(CloudSpatialAnchor anchor)
        {
            // Validate
            if (anchor == null) throw new ArgumentNullException(nameof(anchor));

            // Store
            this.AnchorId = anchor.Identifier;
            this.cloudAnchor = anchor;

            // Apply
            CloudToNative(anchor);
        }
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets the underlying native anchor.
        /// </summary>
        public CloudSpatialAnchor CloudAnchor { get { return cloudAnchor; } }
        #endregion // Public Properties}
    }
}