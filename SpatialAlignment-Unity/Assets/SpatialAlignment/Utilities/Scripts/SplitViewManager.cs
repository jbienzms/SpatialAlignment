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

using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using UnityEngine;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// Defines the various modes of <see cref="SplitViewManager"/>.
    /// </summary>
    public enum SplitViewMode
    {
        /// <summary>
        /// No portion of the view is occluded.
        /// </summary>
        Unoccluded,
        /// <summary>
        /// The right half of the view is occluded.
        /// </summary>
        OccludedRight,
        /// <summary>
        /// The left half of the view is occluded.
        /// </summary>
        OccludedLeft,
        /// <summary>
        /// The top of the view is occluded.
        /// </summary>
        OccludedTop,
        /// <summary>
        /// The bottom half of the view is occluded.
        /// </summary>
        OccludedBottom
    }

    /// <summary>
    /// Controls the rendering of a split holographic / non-holographic view.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class SplitViewManager : MonoBehaviour
    {
        #region Member Variables
        private SplitViewMode lastMode;
        private MeshRenderer meshRenderer;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("Controls which part of the view is occluded.")]
        private SplitViewMode mode;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        private void ApplyMode()
        {
            float startingZ = transform.localPosition.z;

            switch (mode)
            {
                case SplitViewMode.Unoccluded:

                    meshRenderer.enabled = false;
                    break;

                case SplitViewMode.OccludedRight:

                    transform.localPosition = new Vector3(0.5f, 0f, startingZ);
                    meshRenderer.enabled = true;
                    break;

                case SplitViewMode.OccludedLeft:

                    transform.localPosition = new Vector3(-0.5f, 0f, startingZ);
                    meshRenderer.enabled = true;
                    break;

                case SplitViewMode.OccludedTop:

                    transform.localPosition = new Vector3(0f, 0.5f, startingZ);
                    meshRenderer.enabled = true;
                    break;

                case SplitViewMode.OccludedBottom:

                    transform.localPosition = new Vector3(0f, -0.5f, startingZ);
                    meshRenderer.enabled = true;
                    break;

                default:
                    break;
            }
        }

        private void GatherComponents()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                Debug.LogError($"A {nameof(MeshRenderer)} component is required but none was found. {nameof(SplitViewManager)} has been disabled.");
                this.enabled = false;
            }
        }
        #endregion // Internal Methods

        #region Unity Overrides
        /// <inheritdoc />
        protected virtual void Awake()
        {
            GatherComponents();
            if (enabled)
            {
                ApplyMode();
            }
        }

        /// <inheritdoc />
        protected virtual void Start()
        {
            // Attempt to re-parent to camera
            var mainCamera = CameraCache.Main;
            if (mainCamera != null)
            {
                this.transform.SetParent(mainCamera.transform, worldPositionStays: false);
            }
        }

        /// <inheritdoc />
        protected virtual void Update()
        {
            if (lastMode != mode)
            {
                lastMode = mode;
                ApplyMode();
            }
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets a value which controls which part of the view is occluded.
        /// </summary>
        public SplitViewMode Mode { get { return mode; } set { mode = value; } }
        #endregion // Public Properties
    }
}
