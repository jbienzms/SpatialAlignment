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
    /// Provides heading data for updates to <see cref="IGeoReference"/> sources.
    /// </summary>
    public class HeadingData
    {
        #region Member Variables
        private float northHeading;
        private float northAccuracy;
        #endregion // Member Variables

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="LocationData"/> instance.
        /// </summary>
        /// <param name="northHeading">
        /// The <see cref="NorthHeading"/>.
        /// </param>
        /// <param name="northAccuracy">
        /// The <see cref="NorthAccuracy"/>.
        /// </param>
        public HeadingData(float northHeading, float northAccuracy)
        {
            // Store
            this.northHeading = northHeading;
            this.northAccuracy = northAccuracy;
        }
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets the rotation angle (in degrees) at the reference position
        /// which represents true north.
        /// </summary>
        public float NorthHeading { get => northHeading; }

        /// <summary>
        /// Gets the accuracy of the rotation angle for true north.
        /// </summary>
        public float NorthAccuracy { get => northAccuracy; }
        #endregion // Public Properties
    }
}
