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

using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Receivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// Handles user interaction and routes it to a <see cref="NudgeRefinement"/>.
    /// </summary>
    public class NudgeController : InteractionReceiver
    {
        #region Nested Types
        private enum NudgeAction
        {
            Direction,
            Rotation,
            Finish,
            Cancel
        };
        #endregion // Nested Types

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The nudge refinement instance to control.")]
        private NudgeRefinement refinement;
        #endregion // Unity Inspector Variables

        #region Overrides / Event Handlers
        protected override void InputUp(GameObject obj, InputEventData eventData)
        {
            if (refinement == null)
            {
                Debug.LogWarning($"{nameof(NudgeController)} does not have a valid {nameof(Refinement)} instance.");
                return;
            }

            // What action?
            NudgeAction action;
            RefinementDirection direction;
            NudgeRotation rotation;

            switch (obj.name)
            {
                case "Ok":
                    action = NudgeAction.Finish;
                    break;
                case "Cancel":
                    action = NudgeAction.Cancel;
                    break;

            }

            Debug.Log(obj.name + " : InputUp");
            txt.text = obj.name + " : InputUp";
        }
        #endregion // Overrides / Event Handlers

        #region Public Properties
        /// <summary>
        /// Gets or sets the nudge refinement instance to control.
        /// </summary>
        public NudgeRefinement Refinement { get { return refinement; } set { refinement = value; } }
        #endregion // Public Properties
    }
}
