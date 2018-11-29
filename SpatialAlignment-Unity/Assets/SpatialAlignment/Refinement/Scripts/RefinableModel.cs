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

using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using HoloToolkit.Unity.Receivers;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity.UX;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// Defines the various modes of a <see cref="RefinableModel"/>.
    /// </summary>
    public enum RefinableModelMode
    {
        /// <summary>
        /// The model is in the default "placed" mode.
        /// </summary>
        Placed,

        /// <summary>
        /// The model is in the process of being placed. <see cref="Placing"/>
        /// is usually a quick "rough draft" positioning of the object
        /// that is then followed up with <see cref="Refining"/>.
        /// </summary>
        Placing,

        /// <summary>
        /// The position or orientation of the model is being refined.
        /// </summary>
        Refining
    }

    /// <summary>
    /// Represents an model in space that can be positioned and
    /// updated by the user interactively.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class RefinableModel : RefinementController
    {
        #region Member Variables
        private BoundingBoxRig boundingBoxRig;
        private Collider modelCollider;
        private bool firstUpdate = true;
        private RefinableModelMode lastMode;
        private TapToPlace tapToPlace;
        private TwoHandManipulatable twoHandManipulatable;
        private bool wasBeingPlaced;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [Header("Behavior")]
        [SerializeField]
        [Tooltip("The manipulation modes that should be supported.")]
        private ManipulationMode manipulationMode = ManipulationMode.MoveScaleAndRotate;

        [SerializeField]
        [Tooltip("Constrain rotation along an axis")]
        private AxisConstraint rotationConstraint = AxisConstraint.None;

        [SerializeField]
        [Tooltip("The refinement mode of the current model.")]
        private RefinableModelMode refinementMode;

        [SerializeField]
        [Tooltip("Whether to use bounding box when in refinement mode.")]
        private bool useBoundingBoxRig = true;

        [SerializeField]
        [Tooltip("Whether to use two hand manipulation when in refinement mode.")]
        private bool useTwoHand = true;

        [Header("Preset Components")]
        [SerializeField]
        [Tooltip("The prefab to instantiate when an AppBar is needed. A default exists at MixedRealityToolkit/UX/Prefabs/AppBar/AppBar.prefab.")]
        private AppBar appBarPrefab;

        [SerializeField]
        [Tooltip("The prefab to instantiate when a bounding box is needed. A default exists at MixedRealityToolkit/UX/Prefabs/BoundingBoxes/BoundingBoxBasic.prefab.")]
        private BoundingBox boundingBoxPrefab;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Disables <see cref="BoundingBoxRig"/>.
        /// </summary>
        private void DisableBoundingBox()
        {
            if (boundingBoxRig != null)
            {
                // TODO: Destroy AppBar

                // Destroy rig
                Destroy(boundingBoxRig);
                boundingBoxRig = null;
            }
        }

        /// <summary>
        /// Disables <see cref="TapToPlace"/>.
        /// </summary>
        private void DisableTapToPlace()
        {
            if (tapToPlace != null)
            {
                // Get rid of tap to place
                Destroy(tapToPlace);

                // Collider can be re-enabled now
                modelCollider.enabled = true;
            }
        }

        /// <summary>
        /// Disables <see cref="TwoHandManipulatable"/>.
        /// </summary>
        private void DisableTwoHand()
        {
            if (twoHandManipulatable != null)
            {
                Destroy(twoHandManipulatable);
            }
        }

        /// <summary>
        /// Enables <see cref="BoundingBoxRig"/>.
        /// </summary>
        private void EnableBoundingBox()
        {
            // Make sure not already enabled
            if (boundingBoxRig != null) { return; }

            // Instantiate the bounding box rig
            boundingBoxRig = gameObject.AddComponent<BoundingBoxRig>();

            // Setup the prefabs
            boundingBoxRig.AppBarPrefab = appBarPrefab;
            boundingBoxRig.BoundingBoxPrefab = boundingBoxPrefab;

            // Disable scaling?
            if ((manipulationMode & ManipulationMode.Scale) != ManipulationMode.Scale)
            {
                boundingBoxRig.ScaleRate = 0;
            }

            // TODO: Add menu buttons
        }

        /// <summary>
        /// Enables <see cref="TapToPlace"/>.
        /// </summary>
        private void EnableTapToPlace()
        {
            // Make sure not already enabled
            if (tapToPlace != null) { return; }

            // Add component
            tapToPlace = gameObject.AddComponent<TapToPlace>();

            // Don't modify the mesh
            tapToPlace.AllowMeshVisualizationControl = false;

            // Collider can't be on while TapToPlace is on
            modelCollider.enabled = false;

            // Start placing
            tapToPlace.IsBeingPlaced = true;
        }

        /// <summary>
        /// Enables <see cref="TwoHandManipulatable"/>.
        /// </summary>
        private void EnableTwoHand()
        {
            // Make sure not already enabled
            if (twoHandManipulatable != null) { return; }

            // Add component
            twoHandManipulatable = gameObject.AddComponent<TwoHandManipulatable>();

            // Set manipulations and constraints
            twoHandManipulatable.ManipulationMode = manipulationMode;
            twoHandManipulatable.RotationConstraint = rotationConstraint;

            // Allow single hand as well
            twoHandManipulatable.EnableEnableOneHandedMovement = true;
        }

        /// <summary>
        /// Gathers all dependency components and disables the behavior if a
        /// dependency is not found.
        /// </summary>
        private void GatherDependencies()
        {
            // Get components
            modelCollider = GetComponent<Collider>();

            // Validate components
            if (modelCollider == null)
            {
                Debug.LogError($"A {nameof(Collider)} component is required but none was found. {nameof(RefinableModel)} has been disabled.");
                this.enabled = false;
            }

            if (appBarPrefab == null)
            {
                Debug.LogError($"{nameof(appBarPrefab)} is required but is not set. {nameof(RefinableModel)} has been disabled.");
                this.enabled = false;
            }

            if (boundingBoxPrefab == null)
            {
                Debug.LogError($"{nameof(boundingBoxPrefab)} is required but is not set. {nameof(RefinableModel)} has been disabled.");
                this.enabled = false;
            }

            //// Add a 'Finished' button
            //List<AppBar.ButtonTemplate> buttons = boundingBoxRig.AppBarPrefab.Buttons.ToList();
            //buttons.Add(new AppBar.ButtonTemplate()
            //{
            //    DefaultPosition = 1,
            //    ManipulationPosition = 1,
            //    Type = AppBar.ButtonTypeEnum.Custom,
            //    Name = "Finished",
            //    Icon = "AppBarDone",
            //    Text = "Finished",
            //    EventTarget = this,
            //});
            //boundingBoxRig.AppBarPrefab.Buttons = buttons.ToArray();
        }

        /// <summary>
        /// Switches the UI and interactive elements of the model to enable
        /// the specified mode.
        /// </summary>
        /// <param name="newMode">
        /// The mode to switch to.
        /// </param>
        protected virtual void SwitchMode(RefinableModelMode newMode)
        {
            switch (newMode)
            {
                case RefinableModelMode.Placed:

                    // Disable all components
                    DisableBoundingBox();
                    DisableTapToPlace();
                    DisableTwoHand();

                    break;

                case RefinableModelMode.Placing:

                    // Disable bounds and hands
                    DisableBoundingBox();
                    DisableTwoHand();

                    // Enable Tap to Place
                    EnableTapToPlace();
                    break;

                case RefinableModelMode.Refining:

                    // Disable TapToPlace
                    DisableTapToPlace();

                    // Enable which components?
                    if (useBoundingBoxRig) { EnableBoundingBox(); }
                    if (useTwoHand) { EnableTwoHand(); }
                    break;

                default:
                    throw new InvalidOperationException("Unexpected Branch");
            }

            // Store new mode
            lastMode = newMode;
            refinementMode = newMode;

            // Notify
            OnModeChanged();
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers

        protected override void OnRefinementCanceled()
        {
            base.OnRefinementCanceled();
            RefinementMode = RefinableModelMode.Placed;
        }

        protected override void OnRefinementFinished()
        {
            RefinementMode = RefinableModelMode.Placed;
            base.OnRefinementFinished();
        }

        protected override void OnRefinementStarted()
        {
            base.OnRefinementStarted();
            if (refinementMode == RefinableModelMode.Placed)
            {
                refinementMode = RefinableModelMode.Placing;
            }
        }

        /*
        /// <inheritdoc />
        protected override void InputClicked(GameObject obj, InputClickedEventData eventData)
        {
            // Pass to base first
            base.InputClicked(obj, eventData);

            // If it's any of our buttons, do the right action.
            if (obj.name == "Finish")
            {
                FinishRefinement();
            }
            else if (obj.name == "Cancel")
            {
                CancelRefinement();
            }
        }
        */
        #endregion // Overrides / Event Handlers

        #region Overridables / Event Triggers
        /// <summary>
        /// Called when the value of the <see cref="RefinementMode"/> property has
        /// changed.
        /// </summary>
        protected virtual void OnModeChanged()
        {
            RefinementModeChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion // Overridables / Event Triggers

        #region Unity Overrides
        /// <inheritdoc />
        protected override void Start()
        {
            // Pass to base first
            base.Start();

            // Gather dependencies
            GatherDependencies();

            // Switch to the starting mode to ensure
            // all visuals are set correctly
            SwitchMode(refinementMode);
        }

        /// <inheritdoc />
        protected override void Update()
        {
            // Pass to base first
            base.Update();

            // Check for a mode change from editor inspector
            if ((lastMode != refinementMode) || (firstUpdate))
            {
                firstUpdate = false;
                SwitchMode(refinementMode);
            }

            // Check for tap-to-place complete
            if (tapToPlace != null)
            {
                if ((wasBeingPlaced) && (!tapToPlace.IsBeingPlaced))
                {
                    SwitchMode(RefinableModelMode.Placed);
                }
                wasBeingPlaced = tapToPlace.IsBeingPlaced;
            }
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets the prefab to instantiate when an AppBar is needed.
        /// </summary>
        public AppBar AppBarPrefab { get { return appBarPrefab; } set { appBarPrefab = value; } }

        /// <summary>
        /// Gets or sets the prefab to instantiate when a bounding box is needed.
        /// </summary>
        public BoundingBox BoundingBoxPrefab { get { return boundingBoxPrefab; } set { boundingBoxPrefab = value; } }

        /// <summary>
        /// Gets or sets the manipulation modes that should be supported.
        /// </summary>
        public ManipulationMode ManipulationMode { get { return manipulationMode; } set { manipulationMode = value; } }

        /// <summary>
        /// Gets or sets a constraint for rotation along an axis.
        /// </summary>
        public AxisConstraint RotationConstraint { get { return rotationConstraint; } set { rotationConstraint = value; } }

        /// <summary>
        /// Gets or sets the current mode of the model.
        /// </summary>
        public RefinableModelMode RefinementMode
        {
            get
            {
                return refinementMode;
            }
            set
            {
                refinementMode = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to use a bounding box rig when in refinement mode.
        /// </summary>
        public bool UseBoundingBoxRig { get { return useBoundingBoxRig; } set { useBoundingBoxRig = value; } }

        /// <summary>
        /// Gets or sets Whether to use two hand manipulation when in refinement mode.
        /// </summary>
        public bool UseTwoHand { get { return useTwoHand; } set { useTwoHand = value; } }
        #endregion // Public Properties

        #region Public Events
        /// <summary>
        /// Raised when the value of the <see cref="RefinementMode"/> property has
        /// changed.
        /// </summary>
        public event EventHandler RefinementModeChanged;
        #endregion // Public Events
    }
}