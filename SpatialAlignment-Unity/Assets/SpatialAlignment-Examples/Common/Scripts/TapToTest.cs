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

public class TapToTest : InputSystemGlobalHandlerListener, IMixedRealityInputHandler, IMixedRealityInputActionHandler
{
    /// <inheritdoc />
    protected override void RegisterHandlers()
    {
        InputSystem.RegisterHandler<IMixedRealityInputHandler>(this);
        InputSystem.RegisterHandler<IMixedRealityInputActionHandler>(this);
    }

    /// <inheritdoc />
    protected override void UnregisterHandlers()
    {
        InputSystem.UnregisterHandler<IMixedRealityInputHandler>(this);
        InputSystem.UnregisterHandler<IMixedRealityInputActionHandler>(this);
    }

    public void OnActionEnded(BaseInputEventData eventData)
    {
        Debug.Log($"Action ENDED: {eventData.MixedRealityInputAction.Description}");
    }

    public void OnActionStarted(BaseInputEventData eventData)
    {
        Debug.Log($"Action STARTED: {eventData.MixedRealityInputAction.Description}");
    }

    public void OnInputDown(InputEventData eventData)
    {
        Debug.Log($"Input DOWN: {eventData.MixedRealityInputAction.Description}");
    }

    public void OnInputUp(InputEventData eventData)
    {
        Debug.Log($"Input UP: {eventData.MixedRealityInputAction.Description}");
    }
}
