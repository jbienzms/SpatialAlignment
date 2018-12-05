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
        #region Member Variables
        private NudgeController controller;     // The controller instance, if using one.
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The prefab to instantiate when showing a UX controller.")]
        private NudgeController controllerPrefab;

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

        [SerializeField]
        [Tooltip("Whether to display the UX controller when performing refinement.")]
        private bool useController = true;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Creates the controller instance.
        /// </summary>
        protected virtual void CreateController()
        {
            if (controller != null)
            {
                Debug.LogWarning($"{nameof(CreateController)} called but controller is already created.");
                return;
            }

            if (controllerPrefab == null)
            {
                Debug.LogWarning($"{nameof(CreateController)} called but {nameof(ControllerPrefab)} is not defined.");
                return;
            }

            // Instantiate
            controller = Instantiate(controllerPrefab);
        }

        /// <summary>
        /// Destroys the controller instance.
        /// </summary>
        /// <remarks>
        /// The default implementation destroys the prefabs GameObject.
        /// </remarks>
        protected virtual void DestroyController()
        {
            if (controller != null)
            {
                Destroy(controller.gameObject);
                controller = null;
            }
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers
        /// <inheritdoc />
        protected override void OnRefinementCanceled()
        {
            // Destroy the controller
            DestroyController();

            // Pass to base to finish
            base.OnRefinementCanceled();
        }

        /// <inheritdoc />
        protected override void OnRefinementFinished()
        {
            // Destroy the controller
            DestroyController();

            // Pass to base to notify of finished refinement
            base.OnRefinementFinished();
        }

        /// <inheritdoc />
        protected override void OnRefinementStarted()
        {
            // Using a controller?
            if (useController)
            {
                // Create the controller
                CreateController();

                // If created, configure
                if (controller != null)
                {
                    // Link to our instance
                    controller.Refinement = this;

                    // Show the controller
                    ShowController();
                }
            }

            // Pass to base to notify
            base.OnRefinementStarted();
        }
        #endregion // Overrides / Event Handlers

        #region Unity Overrides
        /// <inheritdoc />
        protected override void OnDestroy()
        {
            // If we have a controller, destroy it too
            DestroyController();

            // Pass to base
            base.OnDestroy();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            // If we have a controller, hide it
            if (controller != null)
            {
                HideController();
            }

            // Pass to base
            base.OnDisable();
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            // If we have a controller and we are refining, show it
            if ((controller != null) && (IsRefining))
            {
                ShowController();
            }

            // Pass to base
            base.OnEnable();
        }

        #endregion // Unity Overrides

        #region Public Methods
        /// <summary>
        /// Hides the controller, if one is in use.
        /// </summary>
        /// <remarks>
        /// The default implementation deactivates the controllers GameObject.
        /// </remarks>
        public virtual void HideController()
        {
            if (controller == null)
            {
                Debug.LogWarning($"{nameof(HideController)} called but no controller is in use.");
                return;
            }

            // Deactivate
            controller.gameObject.SetActive(false);
        }

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

        /// <summary>
        /// Shows the controller, if one is in use.
        /// </summary>
        /// <remarks>
        /// The default implementation activates the controllers GameObject.
        /// </remarks>
        public virtual void ShowController()
        {
            if (controller == null)
            {
                Debug.LogWarning($"{nameof(ShowController)} called but no controller is in use.");
                return;
            }

            // Deactivate
            controller.gameObject.SetActive(true);
        }
        #endregion // Public Methods

        #region Public Properties
        /// <summary>
        /// Gets the UX controller instance, if one is active.
        /// </summary>
        public NudgeController Controller { get => controller; protected set => controller = value; }

        /// <summary>
        /// Gets or sets the prefab to instantiate when showing a UX controller.
        /// </summary>
        public NudgeController ControllerPrefab { get { return controllerPrefab; } set { controllerPrefab = value; } }

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
        /// Gets a value that indicates if the controller is currently shown.
        /// </summary>
        public virtual bool IsControllerShown
        {
            get
            {
                return ((controller != null) && (controller.isActiveAndEnabled));
            }
        }

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

        /// <summary>
        /// Gets or sets whether to display the UX controller when performing refinement.
        /// </summary>
        public bool UseController { get { return useController; } set { useController = value; } }
        #endregion // Public Properties
    }
}