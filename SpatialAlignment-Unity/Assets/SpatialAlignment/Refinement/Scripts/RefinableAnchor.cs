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
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// Defines the various modes of a <see cref="RefinableAnchor"/>.
    /// </summary>
    public enum RefinableAnchorMode
    {
        /// <summary>
        /// The anchor is in the default "placed" mode.
        /// </summary>
        Placed,

        /// <summary>
        /// The anchor is in the process of being placed.
        /// </summary>
        Placing,

        /// <summary>
        /// The anchor position or orientation is being refined.
        /// </summary>
        Refining
    }

    /// <summary>
    /// Represents an anchor in space that can be positioned and
    /// updated by the user interactively.
    /// </summary>
    [RequireComponent(typeof(BoundingBoxRig))]
    [RequireComponent(typeof(TapToPlace))]
    [RequireComponent(typeof(TwoHandManipulatable))]
    public class RefinableAnchor : MonoBehaviour
    {
        #region Member Variables
        private BoundingBoxRig boundingBoxRig;
        private RefinableAnchorMode mode;
        private TapToPlace tapToPlace;
        private TwoHandManipulatable twoHandManipulatable;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The mode that the anchor should start in.")]
        private RefinableAnchorMode startMode;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Gathers all dependency components and disables the behavior if a
        /// dependency is not found.
        /// </summary>
        protected virtual void GatherComponents()
        {
            boundingBoxRig = GetComponent<BoundingBoxRig>();
            tapToPlace = GetComponent<TapToPlace>();
            twoHandManipulatable = GetComponent<TwoHandManipulatable>();
            if (boundingBoxRig == null)
            {
                Debug.LogError($"A {nameof(BoundingBoxRig)} component is required but none was found. {nameof(RefinableAnchor)} has been disabled.");
                this.enabled = false;
            }
            if (tapToPlace == null)
            {
                Debug.LogError($"A {nameof(TapToPlace)} component is required but none was found. {nameof(RefinableAnchor)} has been disabled.");
                this.enabled = false;
            }
            if (twoHandManipulatable == null)
            {
                Debug.LogError($"A {nameof(TwoHandManipulatable)} component is required but none was found. {nameof(RefinableAnchor)} has been disabled.");
                this.enabled = false;
            }
        }

        /// <summary>
        /// Switches the UI and interactive elements of the anchor to enable
        /// the specified mode.
        /// </summary>
        /// <param name="newMode">
        /// The mode to switch to.
        /// </param>
        protected virtual void SwitchMode(RefinableAnchorMode newMode)
        {
            switch (newMode)
            {
                case RefinableAnchorMode.Placed:
                    // Disable all
                    boundingBoxRig.enabled = false;
                    tapToPlace.enabled = false;
                    twoHandManipulatable.enabled = false;
                    break;

                case RefinableAnchorMode.Placing:
                    // Disable bounds and hands
                    boundingBoxRig.enabled = false;
                    twoHandManipulatable.enabled = false;

                    // Enable and start Tap to Place
                    tapToPlace.enabled = true;
                    tapToPlace.IsBeingPlaced = true;
                    break;

                case RefinableAnchorMode.Refining:
                    // Stop and disable Tap to Place
                    tapToPlace.IsBeingPlaced = false;
                    tapToPlace.enabled = false;

                    // Enable bounds and hands
                    boundingBoxRig.enabled = true;
                    twoHandManipulatable.enabled = true;
                    break;

                default:
                    throw new InvalidOperationException("Unexpected Branch");
            }

            // Store new mode
            mode = newMode;

            // Notify
            OnModeChanged();
        }
        #endregion // Internal Methods

        #region Overridables / Event Triggers
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

            // Switch to the starting mode
            SwitchMode(startMode);
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets the current mode of the anchor.
        /// </summary>
        public RefinableAnchorMode Mode
        {
            get => mode;
            set
            {
                if (mode != value)
                {
                    SwitchMode(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the mode that the anchor should start in.
        /// </summary>
        public RefinableAnchorMode StartMode { get => startMode; set => startMode = value; }
        #endregion // Public Properties

        #region Public Events
        /// <summary>
        /// Raised when the value of the <see cref="Mode"/> property has
        /// changed.
        /// </summary>
        public event EventHandler ModeChanged;
        #endregion // Public Events
    }
}