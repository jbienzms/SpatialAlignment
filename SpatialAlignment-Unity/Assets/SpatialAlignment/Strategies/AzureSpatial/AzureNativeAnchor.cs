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

// #define UNITY_IOS
// #define WINDOWS_UWP

using SpatialServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_IOS
using UnityEngine.XR.iOS;
using NativeAnchor = UnityEngine.XR.iOS.UnityARUserAnchorComponent;
#elif WINDOWS_UWP
using UnityEngine.XR.WSA;
using NativeAnchor = UnityEngine.XR.WSA.WorldAnchor;
#else
using NativeAnchor = UnityEngine.Component;
#endif

namespace Microsoft.SpatialAlignment.Azure
{
    /// <summary>
    /// A behavior that keeps local platform native anchors in sync with an Azure <see cref="CloudSpatialAnchor"/>.
    /// </summary>
    /// <remarks>
    /// For iOS the platform native type is <see cref="UnityARUserAnchorComponent"/>. For
    /// Windows Mixed Reality and HoloLens the native type is <see cref="WorldAnchor"/>. WorldAnchors
    /// can be updated when new cloud data is available, but UnityARUserAnchorComponents need to be
    /// recreated. This behavior manages those updates.
    /// </remarks>
    public class AzureNativeAnchor : NativeAnchor
    {
        #region Member Variables
        private CloudSpatialAnchor cloudAnchor;
        private NativeAnchor nativeAnchor;

        #if UNITY_IOS
        private UnityARSessionNativeInterface arkitSession;
        #endif
        #endregion // Member Variables

        #region Internal Methods
        /// <summary>
        /// Attempts to find a native anchor already on the same GameObject.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the native anchor was found; otherwise <c>false</c>.
        /// </returns>
        private bool FindNative()
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
        private void FindOrCreateNative()
        {
            if (!FindNative())
            {
                // Add the native anchor
                nativeAnchor = gameObject.AddComponent<NativeAnchor>();
            }
        }

        #if UNITY_IOS
        static private Matrix4x4 GetMatrix4x4FromUnityAr4x4(UnityARMatrix4x4 input)
        {
            Matrix4x4 retval = new Matrix4x4(input.column0, input.column1, input.column2, input.column3);
            return retval;
        }
        #endif // UNITY_IOS
        #endregion // Internal Methods

        #region Unity Overrides
        protected virtual void Awake()
        {
            Debug.Log($"##### CouldNativeAnchor Waking");
            #if UNITY_IOS
            // Make sure we've got a handle to the ARKit session
            if (arkitSession == null)
            {
                arkitSession = UnityARSessionNativeInterface.GetARSessionNativeInterface();
            }
            Debug.Log($"##### CouldNativeAnchor Awake. ARKit Session {arkitSession}");
            #endif // UNITY_IOS

            // If there's already a native anchor, go ahead and reference it
            FindNative();
        }
        #endregion // Unity Overrides

        #region Public Methods
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
        public void CloudToNative(CloudSpatialAnchor anchor)
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
        /// Deletes the native anchor.
        /// </summary>
        public void DeleteNative()
        {
            if (FindNative())
            {
                DestroyImmediate(nativeAnchor);
                nativeAnchor = null;
            }
        }

        /// <summary>
        /// Creates or updates the <see cref="CloudSpatialAnchor"/> returned by
        /// <see cref="CloudAnchor"/> to reflect the same data as the native anchor.
        /// </summary>
        /// <remarks>
        /// If no native anchor exists on the game object it will be created.
        /// </remarks>
        public async Task NativeToCloudAsync()
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
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets the cloud version of the anchor.
        /// </summary>
        /// <value>
        /// The cloud version of the anchor.
        /// </value>
        public CloudSpatialAnchor CloudAnchor { get { return cloudAnchor; } }

        /// <summary>
        /// Gets the native version of the anchor.
        /// </summary>
        /// <value>
        /// The native version of the anchor.
        /// </value>
        public NativeAnchor NativeAnchor { get { return nativeAnchor; } }
        #endregion // Public Properties
    }
}