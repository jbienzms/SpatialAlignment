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

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.UI
{
    /// <summary>
    /// Basic press event receiver
    /// </summary>
    public class InteractableInputActionReceiver : ReceiverBase
    {
        #region Member Variables
        private bool hasDown;
        private IMixedRealityInputSystem inputSystem;
        private State lastState;
        #endregion // Member Variables

        #region Unity Inspector Variables
        [Tooltip("The input action to trigger during press and release.")]
        [SerializeField]
        private MixedRealityInputAction inputAction;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        private bool TryGetInputSystem()
        {
            if (inputSystem != null) { return true; }
            return MixedRealityServiceRegistry.TryGetService<IMixedRealityInputSystem>(out inputSystem);
        }
        #endregion // Internal Methods

        #region Constructors
        public InteractableInputActionReceiver(UnityEvent ev) : base(ev, "InputAction")
        {
            // Name = "InputAction";
        }
        #endregion // Constructors

        #region Overrides / Event Handlers
        /// <inheritdoc/>
        public override void OnUpdate(InteractableStates state, Interactable source)
        {
            // Is it changing
            bool changed = state.CurrentState() != lastState;

            // Was it down before?
            bool hadDown = hasDown;

            // Is it down now?
            hasDown = state.GetState(InteractableStates.InteractableStateEnum.Pressed).Value > 0;

            // Is there a state change?
            if (changed && hasDown != hadDown)
            {
                // Do we have an input system and input action?
                if ((inputAction != MixedRealityInputAction.None) && (TryGetInputSystem()))
                {
                    if (hasDown)
                    {
                        // TODO: No way to raise an action handler

                        //inputSystem.HandleEvent<InputActionHandler>()
                        //inputSystem.RaiseOnInputDown(source.sou)
                    }
                    else
                    {
                        //OnRelease.Invoke();
                    }
                }
            }

            // Update last state for next time
            lastState = state.CurrentState();
        }
        #endregion // Overrides / Event Handlers
    }
}
