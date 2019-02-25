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
using System.Runtime.Serialization;
using UnityEngine;

namespace Microsoft.SpatialAlignment.Geocentric
{
    /// <summary>
    /// A strategy that rotates the frame to a specified compass heading.
    /// </summary>
    [DataContract]
    public class HeadingAlignment : GeoAlignmentBase
    {
        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Tooltip("A rotational offset from North that the frame will be aligned to.")]
        [Range(0f, 360f)]
        private float northRotation;
        #endregion // Unity Inspector Variables

        #region Overrides / Event Handlers
        float y;

        /// <inheritdoc />
        protected override void ApplyHeading(HeadingData heading)
        {
            // If we have no reference or data, we're in inhibited state
            if (heading == null)
            {
                State = AlignmentState.Inhibited;
                Debug.LogWarning($"{nameof(HeadingAlignment)} {name}: {nameof(GeoReference)} heading data unavailable - Inhibited.");
                return;
            }

            // Calculate heading as an offset from north
            float calcHeading = heading.NorthHeading + this.northRotation;

            // Apply rotation
            // Debug.Log($"calcHeading: {calcHeading}");
            transform.localRotation = Quaternion.Euler(0, calcHeading, 0);
        }

        protected override void ApplyLocation(LocationData location)
        {
            // Location data is not used.
        }
        #endregion // Overrides / Event Handlers

        #region Public Properties
        /// <summary>
        /// Gets or sets a rotational offset from North that the frame will be
        /// aligned to.
        /// </summary>
        /// <remarks>
        /// Changing this value will cause the transform to be updated.
        /// </remarks>
        public float NorthRotation
        {
            get
            {
                return northRotation;
            }
            set
            {
                // Ensure changing
                if (northRotation != value)
                {
                    northRotation = value;
                    UpdateTransform();
                }
            }
        }
        #endregion // Public Properties
    }
}