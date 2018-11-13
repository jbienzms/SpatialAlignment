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

using Microsoft.SpatialAlignment.Persistence.Json;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.SpatialAlignment.Persistence
{
    /// <summary>
    /// An example manager that shows how to add and edit refinement anchors.
    /// </summary>
    public class RefinementExampleManager : MonoBehaviour
    {
        #region Member Variables
        private RefinableModel newAnchor;
        private SpatialFrameCollection frames = new SpatialFrameCollection();
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The container for all refining anchors.")]
        private GameObject anchorContainer;

        [SerializeField]
        [Tooltip("The prefab used represent an anchor.")]
        private GameObject anchorPrefab;

        [SerializeField]
        [Tooltip("The large-scale model.")]
        private GameObject model;
        #endregion // Unity Inspector Variables

        #region Unity Overrides
        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        protected virtual void Start()
        {
            if (anchorContainer == null)
            {
                anchorContainer = new GameObject("Anchor Container");
            }

            if (anchorPrefab == null)
            {
                Debug.LogError($"{nameof(anchorPrefab)} is required but is not set. {nameof(RefinementExampleManager)} has been disabled.");
                this.enabled = false;
            }

            if (model == null)
            {
                Debug.LogError($"{nameof(model)} is required but is not set. {nameof(RefinementExampleManager)} has been disabled.");
                this.enabled = false;
            }
        }
        #endregion // Unity Overrides

        #region Public Methods
        /// <summary>
        /// Begins the process of adding a refinement anchor.
        /// </summary>
        public void BeginAddRefinement()
        {
            // Hide the large-scale model
            HideModel();

            // Instantiate the anchor prefab
            GameObject anchorGO = GameObject.Instantiate(anchorPrefab);

            // Set the parent
            anchorGO.transform.SetParent(anchorContainer.transform, worldPositionStays: true);

            // Get or create the refining behavior
            newAnchor = anchorGO.GetComponent<RefinableModel>();
            if (newAnchor == null) { newAnchor = anchorGO.AddComponent<RefinableModel>(); }

            // Put the refining behavior in placement mode
            newAnchor.Mode = RefinableModelMode.Placing;
        }

        /// <summary>
        /// Cancels the process of adding a refinement anchor.
        /// </summary>
        public void CancelAddRefinement()
        {
            // Delete the anchor
            DestroyImmediate(newAnchor);
            newAnchor = null;

            // Show the model
            ShowModel();
        }

        /// <summary>
        /// Completes the process of adding a refinement anchor.
        /// </summary>
        public void EndAddRefinement()
        {
            // TODO: Save the anchor

            // Show the model
            ShowModel();
        }

        /// <summary>
        /// Hides all refinement anchors.
        /// </summary>
        public void HideAnchors()
        {

        }

        /// <summary>
        /// Hides the model.
        /// </summary>
        public void HideModel()
        {
            model.SetActive(false);
        }

        /// <summary>
        /// Shows all refinement anchors.
        /// </summary>
        public void ShowAnchors()
        {

        }

        /// <summary>
        /// Shows the model.
        /// </summary>
        public void ShowModel()
        {
            model.SetActive(true);
        }
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets or sets the container for all refining anchors.
        /// </summary>
        public GameObject AnchorContainer { get => anchorContainer; set => anchorContainer = value; }

        /// <summary>
        /// Gets or sets The prefab used represent an anchor.
        /// </summary>
        public GameObject AnchorPrefab { get => anchorPrefab; set => anchorPrefab = value; }

        /// <summary>
        /// Gets or sets the large-scale model.
        /// </summary>
        public GameObject Model { get => model; set => model = value; }
        #endregion // Public Properties
    }
}