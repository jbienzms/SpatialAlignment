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

#if !NO_MRTK
using HoloToolkit.Unity;
#endif

namespace Microsoft.SpatialAlignment.Geocentric
{
    /**********************************************************************
     * Much of the code below is based on the exceptional Wikipedia
     * reference found here:
     *
     * https://en.wikipedia.org/wiki/Geographic_coordinate_conversion#From_geodetic_to_ECEF_coordinates
     *
     *********************************************************************/

    /// <summary>
    /// Converts between Geocentric (GPS) and Unity space.
    /// </summary>
    static public class GeoConverter
    {
        #region Constants
        private const float DegToRad = (Mathf.PI / 180.0f);     // Short for converting degrees to radians
        private const float WGSAxis = 6378137f;                 // WGS axis expressed in meters
        private const float WGSEcc = 0.081819190842622f;        // WGS eccentricity
        #endregion // Constants

        #region Public Methods
        /// <summary>
        /// Converts Geocentric values to an
        /// <see href="https://en.wikipedia.org/wiki/ECEF">ECEF</see>
        /// <see cref="Vector3"/>.
        /// </summary>
        /// <param name="latitude">The geocentric latitude.</param>
        /// <param name="longitude">The geocentric longitude.</param>
        /// <param name="altitude">The geocentric altitude.</param>
        /// <returns>The converted ECEF value.</returns>
        static public Vector3 ToEcef(float latitude, float longitude, float altitude)
        {
            // Lat / Long Sin and Cos in radians
            float latSin = Mathf.Sin(latitude * DegToRad);
            float lonSin = Mathf.Sin(longitude * DegToRad);
            float latCos = Mathf.Cos(latitude * DegToRad);
            float lonCos = Mathf.Cos(longitude * DegToRad);

            // Prime vertical radius
            float n = WGSAxis / Mathf.Sqrt(1.0f - WGSEcc * WGSEcc * latSin * latSin);

            // Each axis
            float x = (n + altitude) * latCos * lonCos;
            float y = (n + altitude) * latCos * lonSin;
            float z = (n * (1.0f - WGSEcc * WGSEcc) + altitude) * latSin;

            // To Vector3
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Converts a <see cref="LocationInfo"/> to an
        /// <see href="https://en.wikipedia.org/wiki/ECEF">ECEF</see>
        /// <see cref="Vector3"/>.
        /// </summary>
        /// <param name="location">The <see cref="LocationInfo"/> to convert.</param>
        /// <returns>The converted ECEF value.</returns>
        static public Vector3 ToEcef(LocationInfo location)
        {
            // Use geocentric version
            return ToEcef(location.latitude, location.longitude, location.altitude);
        }
        #endregion // Public Methods
    }
}
