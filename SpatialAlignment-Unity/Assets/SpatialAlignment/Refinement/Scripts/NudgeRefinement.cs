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

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Microsoft.SpatialAlignment
{

    /// <summary>
    /// Defines the rotations that a nudge can be performed.
    /// </summary>
    public enum NudgeRotation
    {
        /// <summary>
        /// Rotate left.
        /// </summary>
        Left,

        /// <summary>
        /// Rotate right.
        /// </summary>
        Right
    }

    /// <summary>
    /// A controller that refines the transform using small adjustments on
    /// any axis.
    /// </summary>
    public class NudgeRefinement : RefinementBase
    {
        #region Constants

        #endregion // Constants

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The amount to move in directional operations.")]
        private float directionAmount = 0.01f;

        [SerializeField]
        [Tooltip("The axis that should be considered Forward for direction operations.")]
        private RefinementDirection forwardDirection = RefinementDirection.Forward;

        [SerializeField]
        [Tooltip("The amount to rotate in rotational operations.")]
        private float rotationAmount = 3.6f;

        [SerializeField]
        [Tooltip("The coordinate system to use when performing operations.")]
        private Space space = Space.Self;

        [SerializeField]
        [Tooltip("The axis that should be considered Up for rotation operations.")]
        private RefinementDirection upDirection = RefinementDirection.Up;
        #endregion // Unity Inspector Variables


        #region Overrides / Event Handlers
        /// <inheritdoc />
        protected override void OnRefinementCanceled()
        {
            // Pass to base to finish
            base.OnRefinementCanceled();
        }

        /// <inheritdoc />
        protected override void OnRefinementFinished()
        {
            // Pass to base to notify of finished refinement
            base.OnRefinementFinished();
        }

        /// <inheritdoc />
        protected override void OnRefinementStarted()
        {
            // Pass to base to notify
            base.OnRefinementStarted();
        }
        #endregion // Overrides / Event Handlers

        #region Unity Overrides
        /// <inheritdoc />
        protected override void Start()
        {

            // Pass to base to complete startup
            base.Start();
        }

        /// <inheritdoc />
        protected override void Update()
        {
            // Pass to base first
            base.Update();

        }
        #endregion // Unity Overrides

        #region Public Methods
        /// <summary>
        /// Nudges the transform in the specified direction.
        /// </summary>
        /// <param name="direction">
        /// The direction to nudge in.
        /// </param>
        public void Nudge(RefinementDirection direction)
        {
            // Figure out which actual direction
            RefinementDirection actualDireciton = direction.RelativeTo(forwardDirection);

            // Create the offset
            Vector3 offset = actualDireciton.ToVector() * directionAmount;

            // Update the position
            if (space == Space.World)
            {
                gameObject.transform.position += offset;
            }
            else
            {
                gameObject.transform.localPosition += offset;
            }
        }

        /// <summary>
        /// Nudges the transform in the specified rotation.
        /// </summary>
        /// <param name="rotation">
        /// The direction to nudge in.
        /// </param>
        public void Nudge(NudgeRotation rotation)
        {
            // Determine angle
            float angle = (rotation == NudgeRotation.Left ? -rotationAmount : rotationAmount);

            // Update the rotation
            gameObject.transform.Rotate(upDirection.ToVector(), angle, space);
        }
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets or sets the amount to move in directional operations.
        /// </summary>
        /// <remarks>
        /// The default is 0.01 meters.
        /// </remarks>
        public float DirectionAmount { get { return directionAmount; } set { directionAmount = value; } }

        /// <summary>
        /// Gets or sets the axis that should be considered Forward for direction operations.
        /// </summary>
        /// <remarks>
        /// The default is <see cref="RefinementDirection.Forward"/>.
        /// </remarks>
        public RefinementDirection ForwardDirection { get { return forwardDirection; } set { forwardDirection = value; } }

        /// <summary>
        /// Gets or sets the amount to rotate in rotational operations.
        /// </summary>
        /// <remarks>
        /// The default is 3.6 Euler.
        /// </remarks>
        public float RotationAmount { get { return rotationAmount; } set { rotationAmount = value; } }

        /// <summary>
        /// Gets or sets the coordinate system to use when performing operations.
        /// </summary>
        /// <remarks>
        /// The default is <see cref="Space.Self"/>.
        /// </remarks>
        public Space Space { get { return space; } set { space = value; } }

        /// <summary>
        /// Gets or sets the axis that should be considered Up for rotation operations.
        /// </summary>
        /// <remarks>
        /// The default is <see cref="RefinementDirection.Up"/>.
        /// </remarks>
        public RefinementDirection UpDirection { get { return upDirection; } set { upDirection = value; } }
        #endregion // Public Properties
    }
}