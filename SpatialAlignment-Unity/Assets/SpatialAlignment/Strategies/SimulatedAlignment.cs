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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// A stand-in class that can be used to simulate an alignment strategy and it's various states.
    /// </summary>
    /// <remarks>
    /// This strategy doesn't modify the transform of the object it's applied to, it simply
    /// simulates the various states of the <see cref="IAlignmentStrategy"/> interface.
    /// </remarks>
    [DataContract]
    public class SimulatedAlignment : AlignmentStrategy
    {
        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Tooltip("The current simulated accuracy.")]
        private Vector3 currentAccuracy;

        [DataMember]
        [SerializeField]
        [Tooltip("The current simulated state.")]
        private AlignmentState currentState = AlignmentState.Resolved;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Applies the inspector values to the underlying interface.
        /// </summary>
        private void ApplyValues()
        {
            base.Accuracy = currentAccuracy;
            base.State = currentState;
        }
        #endregion // Internal Methods

        #region Unity Overrides
        private void Awake()
        {
            ApplyValues();
        }

        private void OnValidate()
        {
            ApplyValues();
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets the current simulated accuracy.
        /// </summary>
        public Vector3 CurrentAccuracy
        {
            get
            {
                return currentAccuracy;
            }
            set
            {
                currentAccuracy = value;
                base.Accuracy = value;
            }
        }

        /// <summary>
        /// Gets or sets the current simulated state.
        /// </summary>
        public AlignmentState CurrentState
        {
            get
            {
                return base.State;
            }
            set
            {
                currentState = value;
                base.State = value;
            }
        }
        #endregion // Public Properties
    }
}