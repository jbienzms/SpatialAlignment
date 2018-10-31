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
        [Tooltip("The spatial frame that serves as the parent.")]
        private SpatialFrame frame;

        [SerializeField]
        [Tooltip("The minimum accuracy of the parent alignment for it to be considered valid. Zero means always valid.")]
        private Vector3 minimumAccuracy = Vector3.zero;

        [SerializeField]
        [Tooltip("The minimum state of the parent alignment for it to be considered valid. Unresolved means always valid.")]
        private AlignmentState minimuState = AlignmentState.Resolved;

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

        #region Public Methods
        /// <summary>
        /// Returns a value that indicates if the parent should be considered a valid target.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the parent is a valid target; otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The default implementation makes sure the parent isn't null and also checks
        /// <see cref="MinimumAccuracy"/> and <see cref="MinimumState"/>
        /// </remarks>
        public virtual bool IsValidTarget()
        {
            // Can't be null
            if (frame == null) { return false; }

            // Get alignment strategy
            IAlignmentStrategy strategy = frame.AlignmentStrategy;

            // If no strategy, warn
            if (strategy == null)
            {
                Debug.LogWarning($"Parent frame '{frame.ID}' has no alignment strategy.");
            }

            // Check the state
            if (minimuState > AlignmentState.Error)
            {
                // Must have a strategy
                if (strategy == null) { return false; }

                // Strategy state must match minimum
                if (strategy.State < minimuState) { return false; }
            }

            // Check the accuracy
            if (minimumAccuracy != Vector3.zero)
            {
                // Must have a strategy
                if (strategy == null) { return false; }

                // Strategy accuracy match minimum
                if ((minimumAccuracy.x > 0) && (strategy.Accuracy.x > minimumAccuracy.x)) { return false; }
                if ((minimumAccuracy.y > 0) && (strategy.Accuracy.y > minimumAccuracy.y)) { return false; }
                if ((minimumAccuracy.z > 0) && (strategy.Accuracy.z > minimumAccuracy.z)) { return false; }
            }

            // All checks passed
            return true;
        }
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets or sets the spatial frame that serves as the parent.
        /// </summary>
        public SpatialFrame Frame { get => frame; set => frame = value; }

        /// <summary>
        /// Gets or sets the minimum accuracy of the parent alignment for it to be considered
        /// valid.
        /// </summary>
        /// <remarks>
        /// If this value is set to <see cref="Vector3.zero"/>, the parent will
        /// always be considered valid. Otherwise, the parent must have a behavior
        /// attached that implements <see cref="IAlignmentStrategy"/> and that behavior must
        /// return an <see cref="IAlignmentStrategy.Accuracy">Accuracy</see> of this level or
        /// higher to be considered valid.
        /// </remarks>
        public Vector3 MinimumAccuracy { get => minimumAccuracy; set => minimumAccuracy = value; }

        /// <summary>
        /// Gets or sets the minimum state of the parent alignment for it to be considered valid.
        /// </summary>
        /// <remarks>
        /// If this value is set to <see cref="AlignmentState.Unresolved"/>, the parent will
        /// always be considered valid. If this value is set to
        /// <see cref="AlignmentState.Inhibited"/> or higher, the parent must have a behavior
        /// attached that implements <see cref="IAlignmentStrategy"/> and that behavior must
        /// return a <see cref="IAlignmentStrategy.State">State</see> of this level or higher
        /// to be considered valid.
        /// </remarks>
        public AlignmentState MinimumState { get => minimuState; set => minimuState = value; }

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