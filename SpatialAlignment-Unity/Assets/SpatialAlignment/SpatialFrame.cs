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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// Designates a spatial frame of reference.
    /// </summary>
    /// <remarks>
    /// As mentioned in the article
    /// <see href="https://docs.microsoft.com/en-us/windows/mixed-reality/coordinate-systems">
    /// Coordinate Systems</see>, large-scale mixed reality applications may make use of
    /// more than one frame of reference. The <see cref="ISpatialFrame"/> interface is used
    /// to represent one of potentially many frames of reference.
    /// </remarks>
    [DataContract]
    public class SpatialFrame : MonoBehaviour
    {
        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Tooltip("A unique ID for the spatial frame.")]
        private string id;
        #endregion // Unity Inspector Variables

        #region Overridables / Event Triggers
        /// <summary>
        /// Occurs when the value of the <see cref="Id"/> property has changed.
        /// </summary>
        protected virtual void OnIdChanged()
        {
            this.IdChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion // Overridables / Event Triggers

        #region Public Properties
        /// <summary>
        /// Gets the <see cref="IAlignmentStrategy"/> that is being used to align the frame.
        /// </summary>
        [DataMember]
        public virtual IAlignmentStrategy AlignmentStrategy { get => GetComponent<IAlignmentStrategy>(); }

        /// <summary>
        /// Gets or sets a unique ID for the frame.
        /// </summary>
        /// <remarks>
        /// A unique ID for the frame.
        /// </remarks>
        public virtual string Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnIdChanged();
                }
            }
        }
        #endregion // Public Properties

        #region Public Methods
        /// <summary>
        /// Raised when the value of the <see cref="Id"/> property has changed.
        /// </summary>
        public event EventHandler IdChanged;
        #endregion // Public Methods
    }
}
