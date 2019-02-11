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

namespace Microsoft.SpatialAlignment.Geocentric
{
    /// <summary>
    /// Provides data for updates to <see cref="IGeoReference"/> sources.
    /// </summary>
    public class GeoReferenceData
    {
        #region Member Variables
        private LocationInfo geoPosition;
        private float horizontalAccuracy;
        private Vector3 localPosition;
        private float verticalAccuracy;
        #endregion // Member Variables

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="GeoReferenceData"/> instance.
        /// </summary>
        /// <param name="geoPosition">
        /// The <see cref="GeoPosition"/>.
        /// </param>
        /// <param name="localPosition">
        /// The <see cref="LocalPosition"/>.
        /// </param>
        /// <param name="horizontalAccuracy">
        /// The <see cref="HorizontalAccuracy"/>.
        /// </param>
        /// <param name="verticalAccuracy">
        /// The <see cref="VerticalAccuracy"/>.
        /// </param>
        public GeoReferenceData(LocationInfo geoPosition, Vector3 localPosition, float horizontalAccuracy, float verticalAccuracy)
        {
            // Store
            this.geoPosition = geoPosition;
            this.localPosition = localPosition;
            this.horizontalAccuracy = horizontalAccuracy;
            this.verticalAccuracy = verticalAccuracy;
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets the current
        /// <see cref="https://en.wikipedia.org/wiki/World_Geodetic_System">Geodetic</see>
        /// position for the point of reference.
        /// </summary>
        public LocationInfo GeoPosition { get => geoPosition; }

        /// <summary>
        /// Horizontal accuracy in meters.
        /// </summary>
        public float HorizontalAccuracy { get => horizontalAccuracy; }

        /// <summary>
        /// Gets a the current
        /// <see cref="https://docs.unity3d.com/ScriptReference/Transform-position.html">Local</see>
        /// coordinate system position for the point of reference.
        /// </summary>
        public Vector3 LocalPosition { get => localPosition; }

        /// <summary>
        /// Vertical accuracy in meters.
        /// </summary>
        public float VerticalAccuracy { get => verticalAccuracy; }
        #endregion // Public Properties
    }
}
