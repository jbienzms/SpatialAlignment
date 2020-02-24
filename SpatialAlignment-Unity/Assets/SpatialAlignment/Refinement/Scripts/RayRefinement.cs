﻿//
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

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

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
    public class RayRefinement : RefinementBase, IMixedRealityInputActionHandler
    {
        #region Constants
        private const float DEF_SCALE = 0.05f;
        #endregion // Constants

        #region Member Variables
        private RayRefinementStep currentStep;      // What step we're on in the refinement
        private GameObject currentTarget;           // Current target to be moved
        private Vector3 lastTargetPosition;			// The last position where a target was placed
        private GameObject modelDirectionGO;        // GameObject instance representing the models direction
        private Vector3 modelDirection;             // Position representing the models direction
        private LineRenderer modelLine;             // Used to render a line pointing from the model origin in the model direction
        private GameObject modelOriginGO;			// GameObject instance representing the models origin
        private Vector3 modelOrigin;		        // Position of the models origin
        private GameObject placementDirectionGO;    // GameObject instance representing the placement direction
        private Vector3 placementDirection;         // Position representing the placement direction
        private LineRenderer placementLine;         // Used to render a line pointing from the placement origin in the placement direction
        private GameObject placementOriginGO;       // GameObject instance representing the placement origin
        private Vector3 placementOrigin;            // Position of the placement origin
        private bool targetPlaced;                  // Whether or not the current target has been placed
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("Whether to automatically show and hide the mesh during placement.")]
        private bool autoHideMeshes = true;

        [Tooltip("The input action that will be used to advance refinement. If one is not specified, 'Select' will be used.")]
        [SerializeField]
        private MixedRealityInputAction advanceAction;

        [SerializeField]
        [Tooltip("The prefab used to represent a direction. If one is not specified, a capsule will be used.")]
        private GameObject directionPrefab;

        [SerializeField]
        [Tooltip("The prefab used to generate a line. If one is not specified, a default will be used.")]
        private LineRenderer linePrefab;

        [SerializeField]
        [Tooltip("Maximum distance from the user to consider when selecting model and placement points.")]
        private float maxDistance = 4.5f;

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
        /// Adds a line renderer to the parent and formats it.
        /// </summary>
        /// <param name="parent">
        /// The parent where the line renderer should be added.
        /// </param>
        /// <returns>
        /// The added line renderer
        /// </returns>
        private LineRenderer AddLine(GameObject parent)
        {
            // Create the line renderer
            LineRenderer line = Instantiate(linePrefab, parent.transform);

            // Make sure it's active (in case disabled due to auto generation)
            line.gameObject.SetActive(true);

            // Set the number of points
            line.positionCount = 2;

            // Done!
            return line;
        }

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

            // Start this target at the same location as the last target
            // or near the user if there is no last target position
            target.transform.position = (lastTargetPosition != Vector3.zero ? lastTargetPosition : CameraCache.Main.transform.position);

            // Make sure it's active (in case inactive due to auto generation)
            target.SetActive(true);

            // Name it
            target.name = name;

            // Store the reference
            currentTarget = target;

            // Target has not been placed
            targetPlaced = false;
        }

        /// <summary>
        /// Initializes the solver, actions and optionally starts placing.
        /// </summary>
        private void Init()
        {
            // If no placement action is specified, try to get the "Select" action
            if (advanceAction == MixedRealityInputAction.None)
            {
                // Try to get the input system
                IMixedRealityInputSystem inputSystem;
                if (MixedRealityServiceRegistry.TryGetService<IMixedRealityInputSystem>(out inputSystem))
                {
                    // Try to get the "Select" action.
                    advanceAction = inputSystem.InputSystemProfile.InputActionsProfile.InputActions.Where(a => a.Description == "Select").FirstOrDefault();
                }
            }

            // surfaceMagnetism.MaxDistance = 5;
            // surfaceMagnetism.RaycastDirection = RaycastDirectionEnum.CameraFacing;
            // surfaceMagnetism.SurfaceNormalOffset = 0;
            // surfaceMagnetism.CloseDistance = 0.1;
            // surfaceMagnetism.OrientatioMode = Full;
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
                    CreateTarget(originPrefab, ref modelOriginGO, currentStep.ToString());

                    // Parent the target
                    modelOriginGO.transform.SetParent(TargetTransform, worldPositionStays: true);

                    break;


                case RayRefinementStep.ModelDirection:

                    // Create the target
                    CreateTarget(directionPrefab, ref modelDirectionGO, currentStep.ToString());

                    // Parent the target
                    modelDirectionGO.transform.SetParent(TargetTransform, worldPositionStays: true);

                    // Add line renderer
                    modelLine = AddLine(modelDirectionGO);

                    break;


                case RayRefinementStep.PlacementOrigin:

                    // Hide meshes?
                    if (autoHideMeshes)
                    {
                        modelLine.enabled = false;
                        this.gameObject.SetMeshesEnabled(enabled: false, inChildren: true);
                    }

                    // Create the target
                    CreateTarget(originPrefab, ref placementOriginGO, currentStep.ToString());

                    break;


                case RayRefinementStep.PlacementDirection:

                    // Create the target
                    CreateTarget(directionPrefab, ref placementDirectionGO, currentStep.ToString());

                    // Add line renderer
                    placementLine = AddLine(placementDirectionGO);

                    break;


                case RayRefinementStep.Refinement:

                    // Re-show meshes?
                    if (autoHideMeshes)
                    {
                        modelLine.enabled = true;
                        this.gameObject.SetMeshesEnabled(enabled: true, inChildren: true);
                    }

                    // Get transform positions
                    Vector3 modelOriginWorld = modelOriginGO.transform.position;
                    Vector3 modelDirectionWorld = modelDirectionGO.transform.position;
                    Vector3 placementOriginWorld = placementOriginGO.transform.position;
                    Vector3 placementDirectionWorld = placementDirectionGO.transform.position;

                    // Calculate the model angle
                    float modelAngle = Mathf.Atan2(modelDirectionWorld.x - modelOriginWorld.x, modelDirectionWorld.z - modelOriginWorld.z) * Mathf.Rad2Deg;

                    // Calculate the placement angle
                    float placementAngle = Mathf.Atan2(placementDirectionWorld.x - placementOriginWorld.x, placementDirectionWorld.z - placementOriginWorld.z) * Mathf.Rad2Deg;

                    // Calculate the model -> placement position offset
                    Vector3 offset = placementOriginWorld - modelOriginWorld;

                    // Calculate the model -> placement rotation offset
                    float rotation = (placementAngle - modelAngle);

                    // Update parent position to align origins
                    TargetTransform.position += offset;

                    // Update parent rotation, but around placement origin
                    TargetTransform.RotateAround(placementOriginWorld, Vector3.up, rotation);

                    // Finish refinement
                    FinishRefinement();

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
            CoreServices.InputSystem?.PopModalInputHandler();

            // No longer in any step
            currentStep = RayRefinementStep.None;

            // TODO: Update transforms on every frame
            // HACK: Save transforms
            modelOrigin = (modelOriginGO != null ?  modelOriginGO.transform.position : Vector3.zero);
            modelDirection = (modelDirectionGO != null ? modelDirectionGO.transform.position : Vector3.zero);
            placementOrigin = (placementOriginGO != null ? placementOriginGO.transform.position : Vector3.zero);
            placementDirection = (placementDirectionGO != null ? placementDirectionGO.transform.position : Vector3.zero);

            // Cleanup resources
            Cleanup(ref modelOriginGO);
            Cleanup(ref modelDirectionGO);
            Cleanup(ref placementOriginGO);
            Cleanup(ref placementDirectionGO);

            // Reset placeholders
            lastTargetPosition = Vector3.zero;
            modelLine = null;
            placementLine = null;
            currentTarget = null;
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers
        /// <inheritdoc />
        private void OnActionEnded(BaseInputEventData eventData)
        {
            // If this event is already handled or it's not the one we care about, ignore
            if ((eventData.used) || (eventData.MixedRealityInputAction != advanceAction)) { return; }

            // If we're not refining, ignore
            if (!IsRefining) { return; }

            // We used it
            eventData.Use();

            // If the current target has been successfully placed at least
            // once, move on to the next step
            if (targetPlaced)
            {
                NextStep();
            }

            // We always handle the event, even if it hasn't been placed
            eventData.Use();
        }

        /// <inheritdoc />
        protected override void OnRefinementCanceled()
        {
            // Re-show meshes?
            if (autoHideMeshes)
            {
                this.gameObject.SetMeshesEnabled(enabled: true, inChildren: true);
            }

            // Cleanup resources
            StopAndCleanup();

            // Pass to base to finish
            base.OnRefinementCanceled();
        }

        /// <inheritdoc />
        protected override void OnRefinementFinished()
        {
            // Cleanup resources
            StopAndCleanup();

            // Pass to base to notify of finished refinement
            base.OnRefinementFinished();
        }

        /// <inheritdoc />
        protected override void OnRefinementStarted()
        {
            // Capture input handler (released in StopAndCleanup)
            CoreServices.InputSystem?.PushModalInputHandler(gameObject);

            // Start
            currentStep = RayRefinementStep.None;
            NextStep();

            // Pass to base to notify
            base.OnRefinementStarted();
        }

        protected override void RegisterHandlers()
        {
            base.RegisterHandlers();
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        }

        protected override void UnregisterHandlers()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
            base.UnregisterHandlers();
        }
        #endregion // Overrides / Event Handlers

        #region IMixedRealityInputHandler Interface
        void IMixedRealityInputActionHandler.OnActionStarted(BaseInputEventData eventData) { }
        void IMixedRealityInputActionHandler.OnActionEnded(BaseInputEventData eventData) { OnActionEnded(eventData); }
        #endregion // IInputClickHandler Interface

        #region Unity Overrides
        /// <inheritdoc />
        protected override void Start()
        {
            // Pass to base first
            base.Start();

            // Initialize input actions
            Init();

            // We do not require input focus
            IsFocusRequired = false;

            // If no custom placement layer has been specified, pick something
            // intelligent.
            if (placementLayers == 0)
            {
                // Try to get the first running spatial mapping observer
                var observer = ((IMixedRealityDataProviderAccess)CoreServices.SpatialAwarenessSystem).GetDataProviders<IMixedRealitySpatialAwarenessObserver>().Where(o => o.IsRunning).FirstOrDefault();

                // Do we have an observer?
                if (observer != null)
                {
                    // Yup. Use its physics layer.
                    placementLayers = 1 << observer.DefaultPhysicsLayer;
                }
                else
                {
                    // Nope. Use default.
                    placementLayers = LayerMask.GetMask("Default");
                }
            }

            // If any prefab has not been specified, create something default
            if (linePrefab == null)
            {
                GameObject lineGO = new GameObject();
                linePrefab = lineGO.AddComponent<LineRenderer>();
                linePrefab.shadowCastingMode = ShadowCastingMode.Off;
                linePrefab.receiveShadows = false;
                linePrefab.allowOcclusionWhenDynamic = false;
                linePrefab.startWidth = 0.02f;
                linePrefab.endWidth = 0.02f;
                linePrefab.numCapVertices = 1;
                linePrefab.alignment = LineAlignment.View;
                lineGO.SetActive(false);
            }
            if (originPrefab == null)
            {
                originPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                originPrefab.transform.SetParent(TargetTransform, worldPositionStays: true);
                originPrefab.GetComponent<Collider>().enabled = false;
                originPrefab.transform.localScale = new Vector3(DEF_SCALE, DEF_SCALE, DEF_SCALE);
                originPrefab.SetActive(false);
            }
            if (directionPrefab == null)
            {
                directionPrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                directionPrefab.transform.SetParent(TargetTransform, worldPositionStays: true);
                directionPrefab.GetComponent<Collider>().enabled = false;
                directionPrefab.transform.localScale = new Vector3(DEF_SCALE, DEF_SCALE, DEF_SCALE);
                directionPrefab.SetActive(false);
            }
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

            // Try to find a new position on the current layer
            RaycastHit hitInfo;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hitInfo, maxDistance, layers))
            {
                // Store new position as last
                lastTargetPosition = hitInfo.point;

                // Tell the target to move to the new position
                currentTarget.transform.position = lastTargetPosition;

                // Target has been placed
                targetPlaced = true;
            }

            // If the current target is a direction target, we need to
            // orientate it and update the line renderer too
            if (isDirection)
            {
                // Get this directions transform
                Transform direction = (isModel ? modelDirectionGO.transform : placementDirectionGO.transform);

                // Get the transform of the origin that matches this direction
                Transform origin = (isModel ? modelOriginGO.transform : placementOriginGO.transform);

                // Calculate the relative offset between the two
                Vector3 relativePos = direction.position - origin.position;

                // Rotate the target direction to point away from the origin
                currentTarget.transform.rotation = Quaternion.FromToRotation(TargetTransform.up, relativePos);

                // Get the line renderer
                LineRenderer line = (isModel ? modelLine : placementLine);

                // Update the points
                line.SetPositions(new Vector3[] { origin.position, direction.position });
            }
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets the input action that will be used to advance refinement of the object.
        /// If one is not specified, 'Select' will be used.
        /// </summary>
        public MixedRealityInputAction AdvanceAction { get => advanceAction; set => advanceAction = value; }

        /// <summary>
        /// Gets or sets whether to automatically show and hide the mesh during placement.
        /// </summary>
        public bool AutoHideMeshes { get { return autoHideMeshes; } set { autoHideMeshes = value; } }

        /// <summary>
        /// Gets or sets the prefab used to represent a direction.
        /// </summary>
        /// <remarks>
        /// If one is not specified, a capsule will be used.
        /// </remarks>
        public GameObject DirectionPrefab { get { return directionPrefab; } set { directionPrefab = value; } }

        /// <summary>
        /// Gets or sets the prefab used to generate a line.
        /// </summary>
        /// <remarks>
        /// If one is not specified, a default will be used.
        /// </remarks>
        public LineRenderer LinePrefab { get { return linePrefab; } set { linePrefab = value; } }

        /// <summary>
        /// Gets or sets the maximum distance from the user to consider when
        /// selecting model and placement points.
        /// </summary>
        public float MaxDistance { get { return maxDistance; } set { maxDistance = value; } }

        /// <summary>
        /// Gets the position of the model direction point.
        /// </summary>
        /// <remarks>
        /// Returns <see cref="Vector3.zero"/> if the model direction point hasn't been placed.
        /// </remarks>
        public Vector3 ModelDirection { get => modelDirection; }

        /// <summary>
        /// Gets or sets the layers that represent the model.
        /// </summary>
        /// <remarks>
        /// Only colliders on these layers will be considered part of the model
        /// when selecting the model origin and direction.
        /// </remarks>
        public LayerMask ModelLayers { get { return modelLayers; } set { modelLayers = value; } }

        /// <summary>
        /// Gets the position of the model origin point.
        /// </summary>
        /// <remarks>
        /// Returns <see cref="Vector3.zero"/> if the model origin point hasn't been placed.
        /// </remarks>
        public Vector3 ModelOrigin { get => modelOrigin;  }

        /// <summary>
        /// Gets or sets The prefab used to represent an origin point.
        /// </summary>
        /// <remarks>
        /// If one is not specified, a sphere will be used.
        /// </remarks>
        public GameObject OriginPrefab { get { return originPrefab; } set { originPrefab = value; } }

        /// <summary>
        /// Gets the position of the placement direction point.
        /// </summary>
        /// <remarks>
        /// Returns <see cref="Vector3.zero"/> if the placement direction point hasn't been placed.
        /// </remarks>
        public Vector3 PlacementDirection { get => placementDirection; }

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

        /// <summary>
        /// Gets the position of the placement origin point.
        /// </summary>
        /// <remarks>
        /// Returns <see cref="Vector3.zero"/> if the placement origin point hasn't been placed.
        /// </remarks>
        public Vector3 PlacementOrigin { get => placementOrigin; }
        #endregion // Public Properties
    }
}