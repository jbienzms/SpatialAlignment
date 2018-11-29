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
        PlacementDirection,

        /// <summary>
        /// The transform is being refined based on the origin and angle.
        /// </summary>
        Refinement,
    }

    /// <summary>
    /// A controller that refines the transform based on an origin point and
    /// a direction.
    /// </summary>
    public class RayRefinement : RefinementController, IInputClickHandler
    {
        #region Constants
        private const float DEF_SCALE = 0.05f;
        #endregion // Constants

        #region Member Variables
        private RayRefinementStep currentStep;		// What step we're on in the refinement
        private GameObject modelOrigin;				// GameObject instance representing the models origin
        private GameObject modelDirection;          // GameObject instance representing the models direction
        private GameObject placementOrigin;         // GameObject instance representing the placement origin
        private GameObject placementDirection;      // GameObject instance representing the placement direction
        private Interpolator targetInterpolator;    // Interpolator used to move the current target
        private bool targetPlaced;					// Whether or not the current target has been placed
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The prefab used to represent a direction. If one is not specified, a capsule will be used.")]
        private GameObject directionPrefab;

        [SerializeField]
        [Tooltip("Maximum distance from the user to consider when selecting model and placement points.")]
        private float maxDistance = 3.0f;

        [SerializeField]
        [Tooltip("The layers that represent the model.")]
        private LayerMask modelLayers = 1 << 0;

        [SerializeField]
        [Tooltip("The prefab used to represent an origin point. If one is not specified, a sphere will be used.")]
        private GameObject originPrefab;

        [SerializeField]
        [Tooltip("The layers that represent potential placement.\n\nIf a layer is not specified the controller will attempt to intelligently select one. If a SpatialMappingManager is available the spatial mesh layer will be used. Otherwise the default physics raycast layer will be used.")]
        private LayerMask placementLayers;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Destroys the object and releases the reference to it.
        /// </summary>
        /// <param name="obj">
        /// The object reference to clean up.
        /// </param>
        private void Cleanup(ref GameObject obj)
        {
            if (obj != null)
            {
                Destroy(obj);
                obj = null;
            }
        }

        /// <summary>
        /// Creates a prefab and stores it in the target reference.
        /// </summary>
        /// <param name="prefab">
        /// The prefab to create.
        /// </param>
        /// <param name="target">
        /// The target reference to store it in.
        /// </param>
        /// <param name="name">
        /// The name of the object to create
        /// </param>
        private void CreateTarget(GameObject prefab, ref GameObject target, string name)
        {
            // Create prefab
            target = GameObject.Instantiate(prefab);

            // Name it
            target.name = name;

            // Ensure it has an interpolator and store the reference
            targetInterpolator = target.EnsureComponent<Interpolator>();

            // Target has not been placed
            targetPlaced = false;
        }

        /// <summary>
        /// Executes the next step in the refinement process.
        /// </summary>
        private void NextStep()
        {
            // If we're already on the last step, just finish up
            if (currentStep == RayRefinementStep.Refinement)
            {
                FinishRefinement();
                return;
            }

            // Increment the step
            currentStep++;

            // Execute
            switch (currentStep)
            {
                case RayRefinementStep.ModelOrigin:

                    // Create the target
                    CreateTarget(originPrefab, ref modelOrigin, currentStep.ToString());

                    break;


                case RayRefinementStep.ModelDirection:

                    // Create the target
                    CreateTarget(directionPrefab, ref modelDirection, currentStep.ToString());

                    break;


                case RayRefinementStep.PlacementOrigin:

                    // Create the target
                    CreateTarget(originPrefab, ref placementOrigin, currentStep.ToString());

                    break;


                case RayRefinementStep.PlacementDirection:

                    // Create the target
                    CreateTarget(directionPrefab, ref placementDirection, currentStep.ToString());

                    break;


                case RayRefinementStep.Refinement:

                    // TODO: Calculate and Apply

                    break;


                default:
                    Debug.LogError($"Unknown {nameof(RayRefinementStep)}: {currentStep}");
                    break;
            }
        }

        /// <summary>
        /// Stops refinement and cleans up any refinement related resources.
        /// </summary>
        private void StopAndCleanup()
        {
            // No longer modal
            InputManager.Instance.PopModalInputHandler();

            // No longer in any step
            currentStep = RayRefinementStep.None;

            // Cleanup resources
            Cleanup(ref modelOrigin);
            Cleanup(ref modelDirection);
            Cleanup(ref placementOrigin);
            Cleanup(ref placementDirection);
            targetInterpolator = null;
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers
        protected virtual void OnInputClicked(InputClickedEventData eventData)
        {
            // If the current target has been successfully placed at least
            // once, move on to the next step
            if (targetPlaced)
            {
                NextStep();
            }

            // We always handle the event, even if it hasn't been placed
            eventData.Use();
        }

        protected override void OnRefinementCanceled()
        {
            StopAndCleanup();
            base.OnRefinementCanceled();
        }

        protected override void OnRefinementFinished()
        {
            // Cleanup resources
            StopAndCleanup();

            // Pass to base to notify of finished refinement
            base.OnRefinementFinished();
        }

        protected override void OnRefinementStarted()
        {
            // Capture input handler (released in StopAndCleanup)
            InputManager.Instance.PushModalInputHandler(gameObject);

            // Start
            currentStep = RayRefinementStep.None;
            NextStep();

            // Pass to base to notify
            base.OnRefinementStarted();
        }
        #endregion // Overrides / Event Handlers

        #region IInputClickHandler Interface
        void IInputClickHandler.OnInputClicked(InputClickedEventData eventData) { OnInputClicked(eventData); }
        #endregion // IInputClickHandler Interface

        #region Unity Overrides
        /// <inheritdoc />
        protected override void Start()
        {
            // If no custom placement layer has been specified, pick something
            // intelligent.
            if (placementLayers == 0)
            {
                // If SpatialMappingManager is valid, use its layer mask
                // Otherwise, use all default raycast layers.
                int mask = (SpatialMappingManager.Instance != null ? SpatialMappingManager.Instance.LayerMask : Physics.DefaultRaycastLayers);
                placementLayers = mask;
            }

            // If no prefabs have been specified, create something default
            if (originPrefab == null)
            {
                originPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                originPrefab.GetComponent<Collider>().enabled = false;
                originPrefab.transform.localScale = new Vector3(DEF_SCALE, DEF_SCALE, DEF_SCALE);
            }
            if (directionPrefab == null)
            {
                directionPrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                directionPrefab.GetComponent<Collider>().enabled = false;
                directionPrefab.transform.localScale = new Vector3(DEF_SCALE, DEF_SCALE, DEF_SCALE);
            }

            // Pass to base to complete startup
            base.Start();
        }

        /// <inheritdoc />
        protected override void Update()
        {
            // Pass to base first
            base.Update();

            // If not moving targets, nothing to do
            if (currentStep == RayRefinementStep.None || currentStep == RayRefinementStep.Refinement) { return; }

            // Is the target a model target or placement target
            bool isModel = (currentStep == RayRefinementStep.ModelOrigin || currentStep == RayRefinementStep.ModelDirection);

            // Is the target an origin or direction?
            bool isDirection = (currentStep == RayRefinementStep.ModelDirection || currentStep == RayRefinementStep.PlacementDirection);

            // What layer mask should we use for movement?
            LayerMask layers = (isModel ? modelLayers : placementLayers);

            // Get camera transform
            Transform cameraTransform = CameraCache.Main.transform;

            // Try to find a point on one of the layers
            RaycastHit hitInfo;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hitInfo, maxDistance, layers))
            {
                // Tell the target to move to the new position
                targetInterpolator.SetTargetPosition(hitInfo.point);

                // If the current target is a direction target, we need to orientate it too
                if (isDirection)
                {
                    // Get the origin that matches this direction
                    Transform origin = (isModel ? modelOrigin.transform : placementOrigin.transform);

                    // Calculate the relative offset between the two
                    Vector3 relativePos = hitInfo.point - origin.position;

                    // Rotate the target to point away from the origin
                    targetInterpolator.SetTargetRotation(Quaternion.FromToRotation(transform.up, relativePos));
                }

                // Target has been placed
                targetPlaced = true;
            }
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets the prefab used to represent a direction.
        /// </summary>
        /// <remarks>
        /// If one is not specified, a capsule will be used.
        /// </remarks>
        public GameObject DirectionPrefab { get { return directionPrefab; } set { directionPrefab = value; } }

        /// <summary>
        /// Gets or sets the maximum distance from the user to consider when
        /// selecting model and placement points.
        /// </summary>
        public float MaxDistance { get { return maxDistance; } set { maxDistance = value; } }

        /// <summary>
        /// Gets or sets the layers that represent the model.
        /// </summary>
        /// <remarks>
        /// Only colliders on these layers will be considered part of the model
        /// when selecting the model origin and direction.
        /// </remarks>
        public LayerMask ModelLayers { get { return modelLayers; } set { modelLayers = value; } }

        /// <summary>
        /// Gets or sets The prefab used to represent an origin point.
        /// </summary>
        /// <remarks>
        /// If one is not specified, a sphere will be used.
        /// </remarks>
        public GameObject OriginPrefab { get { return originPrefab; } set { originPrefab = value; } }

        /// <summary>
        /// Gets or sets the layers that represent potential placement.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only colliders on these layers will be considered when selecting
        /// the placement origin and direction.
        /// </para>
        /// <para>
        /// If a layer is not specified the controller will attempt to
        /// intelligently select one. If a <see cref="SpatialMappingManager"/>
        /// is available the spatial mesh layer will be used. Otherwise the
        /// default physics raycast layer will be used.
        /// </para>
        /// </remarks>
        public LayerMask PlacementLayers { get { return placementLayers; } set { placementLayers = value; } }
        #endregion // Public Properties
    }
}