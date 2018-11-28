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
    /// Defines the various steps of a ray-based refinement.
    /// </summary>
    public enum RayRefinementStep
    {
        /// <summary>
        /// No action is being performed.
        /// </summary>
        None,

        /// <summary>
        /// The user is defining the origin point on the model.
        /// </summary>
        ModelOrigin,

        /// <summary>
        /// The user is defining the direction angle on the model.
        /// </summary>
        ModelDirection,

        /// <summary>
        /// The user is defining the origin for placement.
        /// </summary>
        PlacementOrigin,

        /// <summary>
        /// The user is defining the angle for placement.
        /// </summary>
        PlacementDirection
    }

    /// <summary>
    /// A controller that refines the transform based on an origin point and
    /// a direction.
    /// </summary>
    public class RayRefinement : RefinementController
    {
        #region Member Variables
        private RayRefinementStep currentStep;
        #endregion // Member Variables

        [SerializeField]
        [Tooltip("The collider that represents the model.")]
        private Collider modelCollider;
        /// <summary>
        /// Gets or sets The collider that represents the model
        /// </summary>
        public Collider ModelCollider { get => modelCollider; set => modelCollider = value; }

        #region Internal Methods
        private void NextStep()
        {
            // Increment the step
            currentStep++;

            // Execute
            switch (currentStep)
            {
                case RayRefinementStep.ModelOrigin:
                    break;
                case RayRefinementStep.ModelDirection:
                    break;
                case RayRefinementStep.PlacementOrigin:
                    break;
                case RayRefinementStep.PlacementDirection:
                    break;
                default:
                    Debug.LogError($"Unknown {nameof(RayRefinementStep)}: {currentStep}");
                    break;
            }
        }
        #endregion // Internal Methods
    }
}