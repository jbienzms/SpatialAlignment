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
    [RequireComponent(typeof(BoundingBoxRig))]
    [RequireComponent(typeof(TapToPlace))]
    [RequireComponent(typeof(TwoHandManipulatable))]
    public class RefinableModel : InteractionReceiver
    {
        #region Member Variables
        private BoundingBoxRig boundingBoxRig;
        private RefinableModelMode lastMode;
        private TapToPlace tapToPlace;
        private TwoHandManipulatable twoHandManipulatable;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The mode of the current model.")]
        private RefinableModelMode mode;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        private void EnsureFinishButton()
        {
            //// HACK: Wire up "Finished" button
            //GameObject finishedObject = boundingBoxRig?.AppBarInstance?.interactables.Where(i => i.name == "Finished").FirstOrDefault();
            //if (finishedObject == null)
            //{
            //    Debug.LogError($"An interactible named 'Finished' is required in the AppBar but was not found. {nameof(RefinableModel)} has been disabled.");
            //    this.enabled = false;
            //}
            //else
            //{
            //    Registerinteractable(finishedObject);
            //}
        }

        /// <summary>
        /// Gathers all dependency components and disables the behavior if a
        /// dependency is not found.
        /// </summary>
        protected virtual void GatherComponents()
        {
            // Get components
            boundingBoxRig = GetComponent<BoundingBoxRig>();
            tapToPlace = GetComponent<TapToPlace>();
            twoHandManipulatable = GetComponent<TwoHandManipulatable>();

            // Validate components
            if (boundingBoxRig == null)
            {
                Debug.LogError($"A {nameof(BoundingBoxRig)} component is required but none was found. {nameof(RefinableModel)} has been disabled.");
                this.enabled = false;
            }
            if (tapToPlace == null)
            {
                Debug.LogError($"A {nameof(TapToPlace)} component is required but none was found. {nameof(RefinableModel)} has been disabled.");
                this.enabled = false;
            }
            if (twoHandManipulatable == null)
            {
                Debug.LogError($"A {nameof(TwoHandManipulatable)} component is required but none was found. {nameof(RefinableModel)} has been disabled.");
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
                    // Collapse the AppBar
                    if (boundingBoxRig.AppBarInstance != null)
                    {
                        boundingBoxRig.AppBarInstance.State = AppBar.AppBarStateEnum.Default;
                    }

                    // Deactivate BoundingBoxRig
                    boundingBoxRig.Deactivate();

                    tapToPlace.enabled = false;
                    twoHandManipulatable.enabled = false;
                    break;

                case RefinableModelMode.Placing:
                    // Disable bounds and hands
                    boundingBoxRig.Deactivate();
                    // boundingBoxRig.enabled = false;
                    twoHandManipulatable.enabled = false;

                    // Enable and start Tap to Place
                    tapToPlace.enabled = true;
                    tapToPlace.IsBeingPlaced = true;
                    break;

                case RefinableModelMode.Refining:
                    EnsureFinishButton();

                    // Stop and disable Tap to Place
                    tapToPlace.IsBeingPlaced = false;
                    tapToPlace.enabled = false;

                    // Enable hands and bounds and put in adjust mode
                    twoHandManipulatable.enabled = true;
                    // boundingBoxRig.enabled = true;
                    boundingBoxRig.Activate();
                    break;

                default:
                    throw new InvalidOperationException("Unexpected Branch");
            }

            // Store new mode
            lastMode = newMode;
            mode = newMode;

            // Notify
            OnModeChanged();
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers
        /// <inheritdoc />
        protected override void InputClicked(GameObject obj, InputClickedEventData eventData)
        {
            // Pass to base first
            base.InputClicked(obj, eventData);

            // If it's the Finished button, finish
            if (obj.name == "Finished")
            {
                Finish();
            }
        }
        #endregion // Overrides / Event Handlers

        #region Overridables / Event Triggers
        /// <summary>
        /// Called when the user has finished refining the model.
        /// </summary>
        protected virtual void OnFinished()
        {
            Finished?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when the value of the <see cref="Mode"/> property has
        /// changed.
        /// </summary>
        protected virtual void OnModeChanged()
        {
            ModeChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion // Overridables / Event Triggers

        #region Unity Overrides
        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        protected virtual void Start()
        {
            // Gather dependencies
            GatherComponents();

            // Switch to the starting mode to ensure
            // all visuals are set correctly
            SwitchMode(mode);
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        protected virtual void Update()
        {
            if (lastMode != mode)
            {
                SwitchMode(mode);
            }
        }
        #endregion // Unity Overrides

        /// <summary>
        /// Notifies that the user has finished refining the model.
        /// </summary>
        public void Finish()
        {
            Mode = RefinableModelMode.Placed;
            OnFinished();
        }

        #region Public Properties
        /// <summary>
        /// Gets the current mode of the model.
        /// </summary>
        public RefinableModelMode Mode
        {
            get => mode;
            set
            {
                if (lastMode != value)
                {
                    SwitchMode(value);
                }
            }
        }
        #endregion // Public Properties

        #region Public Events
        /// <summary>
        /// Raised when the user has finished refining the model.
        /// </summary>
        public event EventHandler Finished;

        /// <summary>
        /// Raised when the value of the <see cref="Mode"/> property has
        /// changed.
        /// </summary>
        public event EventHandler ModeChanged;
        #endregion // Public Events
    }
}