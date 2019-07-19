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
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.SpatialAlignment.Persistence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.SpatialAlignment.Azure
{
    /// <summary>
    /// An alignment strategy that attaches the object to an Azure Spatial Anchor.
    /// </summary>
    [DataContract]
    public class AzureAnchorAlignment : AlignmentStrategy, INativePersistence
    {
        #region Static Version
        /**********************************************************************
         * The static version of this class is a workaround for the fact that
         * currently SpatialAlignment isn't implemented as a service. We need
         * a place to keep track of the SpatialAnchorManager and currently
         * running watchers across all instances of AzureAnchorAlignment.
         * For now we will do all this management in static methods of this
         * class until this code can be moved into a service.
         *********************************************************************/
        #region Member Variables
        static private List<string> idsToLocate = new List<string>();
        static private SpatialAnchorManager manager;
        static private CloudSpatialAnchorWatcher watcher;
        #endregion // Member Variables

        #region Internal Methods
        /// <summary>
        /// Ensures that the application has a valid cloud manager instance.
        /// </summary>
        static public void EnsureManager()
        {
            // Do we need to acquire the store?
            if (manager == null)
            {
                // Try to find one already in the scene
                manager = FindObjectOfType<SpatialAnchorManager>();

                // If still not found, create one
                if (manager == null)
                {
                    // We cannot dynamically create the manager and add it to the scene
                    // because its start method has to be called. For now, it MUST be in the scene
                    throw new InvalidOperationException($"{nameof(SpatialAnchorManager)} MUST be in the scene.");
                }

                // Subscribe to anchor located to manage our ID list
                manager.AnchorLocated += AnchorLocated;
            }
        }

        /// <summary>
        /// Ensures we have a  valid cloud manager and valid session.
        /// </summary>
        /// <remarks>
        /// If a session is not running, one will be created.
        /// </remarks>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        static public async Task EnsureManagerSessionAsync()
        {
            // First make sure we have a manager
            EnsureManager();

            // If there's no session, start one
            if (!manager.IsSessionStarted)
            {
                await manager.StartSessionAsync();
            }
        }

        /// <summary>
        /// Starts watching for the specified Anchor ID.
        /// </summary>
        /// <param name="anchorId">
        /// The ID of the anchor to start watching for.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        static private async Task StartWatchingAsync(string anchorId)
        {
            #if UNITY_EDITOR
            Debug.Log("Azure Spatial Anchors does not currently work in Unity Editor. Ignoring request to start watching.");
            return;
            #endif // UNITY_EDITOR

            // If we're already watching this ID, just ignore
            if (idsToLocate.Contains(anchorId)) { return; }

            // Ensure we have a manager and session
            await EnsureManagerSessionAsync();

            // If there is an existing watcher running, stop it
            if (watcher != null)
            {
                watcher.Stop();
                watcher = null;
            }

            // Create new locate criteria
            AnchorLocateCriteria criteria = new AnchorLocateCriteria();

            // Lock for thread safety
            lock (idsToLocate)
            {
                // Add the new ID
                idsToLocate.Add(anchorId);

                // Store all IDs in the new criteria
                criteria.Identifiers = idsToLocate.ToArray();
            }

            // Create a new watcher based on the criteria
            // (which automatically starts watching)
            watcher = manager.Session.CreateWatcher(criteria);
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers
        static private void AnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            // Regardless of status (found, error, etc.) we no longer need to be
            // watching this Anchor ID
            if (idsToLocate.Contains(args.Identifier))
            {
                // Thread safe
                lock (idsToLocate)
                {
                    idsToLocate.Remove(args.Identifier);
                }
            }
        }
        #endregion // Overrides / Event Handlers
        #endregion // Static Version

        #region Instance Version
        #region Member Variables
        private CloudNativeAnchor cloudNativeAnchor;
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
        /// Applies a cloud spatial anchor using a CloudNativeAnchor.
        /// The CNA will be created if it's not found.
        /// </summary>
        /// <param name="anchor">
        /// The <see cref="CloudSpatialAnchor"/> to apply.
        /// </param>
        private void CloudToNative(CloudSpatialAnchor anchor)
        {
            // Validate
            if (anchor == null) throw new ArgumentNullException(nameof(anchor));

            // Make sure we have a CloudNativeAnchor
            if (cloudNativeAnchor == null)
            {
                cloudNativeAnchor = this.gameObject.AddComponent<CloudNativeAnchor>();
            }

            // Do cloud to native
            cloudNativeAnchor.CloudToNative(anchor);

            // We are now found
            State = AlignmentState.Tracking;
        }

        /// <summary>
        /// Subscribe to cloud manager events.
        /// </summary>
        private void SubscribeManager()
        {
            manager.AnchorLocated += Manager_AnchorLocated;
        }

        /// <summary>
        /// Unsubscribe from cloud manager events.
        /// </summary>
        private void UnsubscribeManager()
        {
            manager.AnchorLocated -= Manager_AnchorLocated;
        }

        #endregion // Internal Methods

        #region INativePersistence Members
        Task INativePersistence.LoadNativeAsync()
        {
            return LoadCloudAsync();
        }

        Task INativePersistence.SaveNativeAsync()
        {
            return SaveCloudAsync();
        }
        #endregion // INativePersistence Members

        #region Overrides / Event Handlers
        private void Manager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
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
                    UnityDispatcher.InvokeOnAppThread(() => CloudToNative(args.Anchor));
                }
                // Was it deleted or is it a bad ID?
                else if (args.Status == LocateAnchorStatus.NotLocatedAnchorDoesNotExist)
                {
                    UnityDispatcher.InvokeOnAppThread(() => State = AlignmentState.Error);
                    Debug.LogError($"{nameof(AzureAnchorAlignment)}: {nameof(CloudSpatialAnchor)} with the id '{anchorId}' was not found on the server.");
                }
            }
        }
        #endregion // Overrides / Event Handlers

        #region Unity Overrides
        /// <inheritdoc />
        protected override void OnDisable()
        {
            // If we have a manager, unsubscribe
            if (manager != null) { UnsubscribeManager(); }

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

        /// <inheritdoc />
        protected async override void Start()
        {
            // Pass to base first
            base.Start();

            // If load on start is true and we have an ID, load
            if (loadOnStart)
            {
                if (string.IsNullOrEmpty(anchorId))
                {
                    // Warn about invalid ID
                    Debug.LogWarning($"{nameof(LoadOnStart)} is true, but {nameof(AnchorId)} is invalid. Not loading.");
                }
                else
                {
                    // Attempt to load
                    await LoadCloudAsync();
                }
            }
        }
        #endregion // Unity Overrides

        #region Public Methods
        /// <summary>
        /// Attempts to load the cloud anchor specified by <see cref="AnchorId"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public async Task LoadCloudAsync()
        {
            // Validate the ID
            if (string.IsNullOrEmpty(AnchorId))
            {
                State = AlignmentState.Error;
                Debug.LogError($"{nameof(AzureAnchorAlignment)}: {nameof(AnchorId)} is not valid.");
            }

            // Start watching for the anchor ID
            await StartWatchingAsync(anchorId);
        }

        /// <summary>
        /// Attempts to save the cloud anchor with the specified <see cref="AnchorId"/>.
        /// If the current game object doesn't have an anchor, it will automatically
        /// be created.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public async Task SaveCloudAsync(CancellationToken cancellationToken)
        {
            // If there is no anchor, create it
            if (cloudNativeAnchor == null) { cloudNativeAnchor = gameObject.AddComponent<CloudNativeAnchor>(); }

            #if UNITY_EDITOR
            Debug.Log("Azure Spatial Anchors does not currently work in Unity Editor. Ignoring request to save anchor.");
            return;
            #endif // UNITY_EDITOR

            // Make sure we have the cloud manager and session
            await EnsureManagerSessionAsync();

            // Convert native anchor format to cloud anchor format
            await cloudNativeAnchor.NativeToCloudAsync();

            // Now save cloud anchor
            await manager.CreateAnchorAsync(cloudNativeAnchor.CloudAnchor, cancellationToken);

            // Update our anchor ID to point at cloud ID (which may have just
            // been generated).
            AnchorId = cloudNativeAnchor.CloudAnchor.Identifier;
        }

        /// <summary>
        /// Attempts to save the cloud anchor with the specified <see cref="AnchorId"/>.
        /// If the current game object doesn't have an anchor, it will automatically
        /// be created.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public Task SaveCloudAsync()
        {
            return SaveCloudAsync(CancellationToken.None);
        }
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets or sets the ID of the anchor to load.
        /// </summary>
        public string AnchorId { get { return anchorId; } set { anchorId = value; } }

        /// <summary>
        /// Gets the underlying cloud-native anchor.
        /// </summary>
        public CloudNativeAnchor CloudNativeAnchor { get { return cloudNativeAnchor; } }

        /// <summary>
        /// Gets or sets a value that indicates if the anchor should be loaded when the behavior starts.
        /// </summary>
        public bool LoadOnStart { get { return loadOnStart; } set { loadOnStart = value; } }
        #endregion // Public Properties
        #endregion // Instance Version
    }
}