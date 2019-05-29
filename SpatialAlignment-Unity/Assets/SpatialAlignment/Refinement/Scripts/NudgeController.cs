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
    public class NudgeController : MonoBehaviour
    {
        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The nudge refinement instance to control.")]
        private NudgeRefinement refinement;
        #endregion // Unity Inspector Variables


        public void Finish()
        {
            refinement.FinishRefinement();
        }
        public void Cancel()
        {
            refinement.CancelRefinement();
        }
        public void Up()
        {
            refinement.Nudge(RefinementDirection.Up);
        }
        public void Down()
        {
            refinement.Nudge(RefinementDirection.Down);
        }
        public void Left()
        {
            refinement.Nudge(RefinementDirection.Left);
        }
        public void Right()
        {
            refinement.Nudge(RefinementDirection.Right);
        }
        public void Forward()
        {
            refinement.Nudge(RefinementDirection.Forward);
        }

        public void Back()
        {
            refinement.Nudge(RefinementDirection.Back);
        }

        public void RotateLeft()
        {
            refinement.Nudge(NudgeRotation.Left);
        }

        public void RotateRight()
        {
            refinement.Nudge(NudgeRotation.Right);
        }

        #region Public Properties
        /// <summary>
        /// Gets or sets the nudge refinement instance to control.
        /// </summary>
        public NudgeRefinement Refinement { get { return refinement; } set { refinement = value; } }
        #endregion // Public Properties
    }
}
