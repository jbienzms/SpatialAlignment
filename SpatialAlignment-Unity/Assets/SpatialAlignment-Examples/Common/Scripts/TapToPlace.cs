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

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapToPlace : BaseInputHandler, IMixedRealityPointerHandler
{
    #region Member Variables
    private SurfaceMagnetism surfaceMagnetism;
    #endregion // Member Variables

    #region Unity Inspector Variables
    [Tooltip("Setting this to true will enable the user to move and place the object in the scene without needing to tap on the object. Useful when you want to place an object immediately.")]
    [SerializeField]
    private bool isBeingPlaced;
    #endregion // Unity Inspector Variables

    #region Internal Methods
    /// <summary>
    /// Initializes the solver and optionally starts placing.
    /// </summary>
    private void InitSolver()
    {
        // surfaceMagnetism.MaxDistance = 5;
        // surfaceMagnetism.RaycastDirection = RaycastDirectionEnum.CameraFacing;
        // surfaceMagnetism.SurfaceNormalOffset = 0;
        // surfaceMagnetism.CloseDistance = 0.1;
        // surfaceMagnetism.OrientatioMode = Full;

        // Enable or disable solver
        surfaceMagnetism.enabled = isBeingPlaced;
    }
    #endregion // Internal Methods

    #region Overridables / Event Triggers
    /// <summary>
    /// Called when the value of the <see cref="IsBeingPlaced"/> property
    /// has changed.
    /// </summary>
    protected virtual void OnIsBeingPlacedChaged()
    {
        // Enable or disable the solver
        surfaceMagnetism.enabled = isBeingPlaced;

        // Raise the event
        this.IsBeingPlacedChaged?.Invoke(this, EventArgs.Empty);
    }
    #endregion // Overridables / Event Triggers

    #region Unity Overrides
    protected override void Start()
    {
        // Pass to base first
        base.Start();

        // Get components
        surfaceMagnetism = this.EnsureComponent<SurfaceMagnetism>();

        // Initialize the solver
        InitSolver();
    }
    protected virtual void Update()
    {
        // Track changes in editor
        if ((surfaceMagnetism != null) && (surfaceMagnetism.enabled != isBeingPlaced))
        {
            OnIsBeingPlacedChaged();
        }
    }
    #endregion // Unity Overrides

    #region IMixedRealityPointerHandler
    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        // Currently we're only going to place by tap, not start by tap
        if (IsBeingPlaced)
        {
            IsBeingPlaced = false;
        }
    }

    void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData) { }
    void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData) { }
    void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData) { }
    #endregion // IMixedRealityPointerHandler


    #region Public Properties
    /// <summary>
    /// Gets or sets a value that indicates if the user is moving the object.
    /// Setting this to true in the editor will allow you to place the object immediately.
    /// </summary>
    public bool IsBeingPlaced
    {
        get
        {
            return isBeingPlaced;
        }
        set
        {
            if (value != isBeingPlaced)
            {
                // Update
                isBeingPlaced = value;

                // Notify
                OnIsBeingPlacedChaged();
            }
        }
    }
    #endregion // Public Properties


    #region Public Events
    /// <summary>
    /// Raised when the value of the <see cref="IsBeingPlaced"/> property
    /// has changed.
    /// </summary>
    public event EventHandler IsBeingPlacedChaged;
    #endregion // Public Events
}
