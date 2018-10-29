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
    /// Provides configuration options for aligning an object to a parent.
    /// </summary>
    [Serializable]
    public class ParentAlignmentOptions
    {
        #region Member Variables
        [SerializeField]
        [Tooltip("The GameObject that serves as the parent.")]
        private GameObject parent;

        [SerializeField]
        [Tooltip("Position to use when a child of this parent.")]
        private Vector3 position = Vector3.zero;

        [SerializeField]
        [Tooltip("Rotation to use when a child of this parent.")]
        private Vector3 rotation = Vector3.zero;

        [SerializeField]
        [Tooltip("Scale to use when a child of this parent.")]
        private Vector3 scale = Vector3.one;
        #endregion // Member Variables

        #region Public Properties
        /// <summary>
        /// Gets or sets the GameObject that serves as the parent.
        /// </summary>
        public GameObject Parent { get => parent; set => parent = value; }

        /// <summary>
        /// Gets or sets the position to use when a child of this parent.
        /// </summary>
        public Vector3 Position { get => position; set => position = value; }

        /// <summary>
        /// Gets or sets the rotation to use when a child of this parent.
        /// </summary>
        public Vector3 Rotation { get => rotation; set => rotation = value; }

        /// <summary>
        /// Gets or sets an optional scale offset from the parent.
        /// </summary>
        public Vector3 Scale { get => scale; set => scale = value; }
        #endregion // Public Properties
    }
}