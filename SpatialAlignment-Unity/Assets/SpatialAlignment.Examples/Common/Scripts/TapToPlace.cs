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
using System.Linq;
using UnityEngine;

public class TapToPlace : InputSystemGlobalHandlerListener, IMixedRealityInputActionHandler
{
    #region Member Variables
    private SurfaceMagnetism surfaceMagnetism;
    private bool lastIsBeingPlaced;
    #endregion // Member Variables

    #region Unity Inspector Variables
    [Tooltip("Setting this to true will enable the user to move and place the object in the scene without needing to tap on the object. Useful when you want to place an object immediately.")]
    [SerializeField]
    private bool isBeingPlaced;

    [Tooltip("The input action that will be used to place the object. If one is not specified, 'Select' will be used.")]
    [SerializeField]
    private MixedRealityInputAction placeAction;
    #endregion // Unity Inspector Variables

    #region Internal Methods
    /// <summary>
    /// Initializes the solver, actions and optionally starts placing.
    /// </summary>
    private void Init()
    {
        // If no placement action is specified, try to get the "Select" action
        if (placeAction == MixedRealityInputAction.None)
        {
            // Try to get the input system
            IMixedRealityInputSystem inputSystem;
            if (MixedRealityServiceRegistry.TryGetService<IMixedRealityInputSystem>(out inputSystem))
            {
                // Try to get the "Select" action.
                placeAction = inputSystem.InputSystemProfile.InputActionsProfile.InputActions.Where(a => a.Description == "Select").FirstOrDefault();
            }
        }

        // surfaceMagnetism.MaxDistance = 5;
        // surfaceMagnetism.RaycastDirection = RaycastDirectionEnum.CameraFacing;
        // surfaceMagnetism.SurfaceNormalOffset = 0;
        // surfaceMagnetism.CloseDistance = 0.1;
        // surfaceMagnetism.OrientatioMode = Full;
        surfaceMagnetism.SurfaceNormalOffset = 0.05f;
        surfaceMagnetism.CurrentOrientationMode = SurfaceMagnetism.OrientationMode.SurfaceNormal;
        surfaceMagnetism.KeepOrientationVertical = false;
    }

    private void SetPlacing()
    {
        // Enable or disable the solver
        surfaceMagnetism.enabled = isBeingPlaced;

        // Notify change
        OnIsBeingPlacedChaged();
    }
    #endregion // Internal Methods

    #region Overridables / Event Triggers
    /// <summary>
    /// Called when the value of the <see cref="IsBeingPlaced"/> property
    /// has changed.
    /// </summary>
    protected virtual void OnIsBeingPlacedChaged()
    {
        // Raise the event
        this.IsBeingPlacedChaged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    protected override void RegisterHandlers()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
    }

    /// <inheritdoc />
    protected override void UnregisterHandlers()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
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
        Init();
    }
    protected virtual void Update()
    {
        // Track changes in editor
        if ((surfaceMagnetism != null) && (lastIsBeingPlaced != isBeingPlaced))
        {
            lastIsBeingPlaced = isBeingPlaced;
            SetPlacing();
        }
    }
    #endregion // Unity Overrides

    #region IMixedRealityInputActionHandler Interface
    void IMixedRealityInputActionHandler.OnActionStarted(BaseInputEventData eventData) { }
    void IMixedRealityInputActionHandler.OnActionEnded(BaseInputEventData eventData)
    {
        // Only place if this event hasn't already been used, we're actively placing, and it's the action we care about
        if ((!eventData.used) && (isBeingPlaced) && (eventData.MixedRealityInputAction == placeAction))
        {
            // We used the event
            eventData.Use();

            // If the object had been moved to a valid surface, consider it placed
            if (surfaceMagnetism.OnSurface)
            {
                // No longer placing
                IsBeingPlaced = false;
            }
            else
            {
                Debug.LogWarning($"'{gameObject.name}' tried to finish placing but isn't on a valid surface.");
            }
        }
    }
    #endregion // IMixedRealityInputActionHandler Interface


    #region Public Properties
    /// <summary>
    /// Gets or sets a value that indicates if the user is moving the object.
    /// Setting this to true in the editor will allow you to place the object
    /// immediately.
    /// </summary>
    public bool IsBeingPlaced { get => isBeingPlaced; set => isBeingPlaced = value; }

    /// <summary>
    /// Gets or sets the input action that will be used to place the object.
    /// If one is not specified, 'Select' will be used.
    /// </summary>
    public MixedRealityInputAction PlaceAction { get => placeAction; set => placeAction = value; }
    #endregion // Public Properties


    #region Public Events
    /// <summary>
    /// Raised when the value of the <see cref="IsBeingPlaced"/> property
    /// has changed.
    /// </summary>
    public event EventHandler IsBeingPlacedChaged;
    #endregion // Public Events
}
