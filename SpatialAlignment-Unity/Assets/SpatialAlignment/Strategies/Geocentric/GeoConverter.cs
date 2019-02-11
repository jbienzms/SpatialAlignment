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
        private const double EarthRadiusInMeters = 6371000d;
        private const double MetersPerLatitudeDegree = 111133d;
        #endregion // Constants

        #region Public Methods
        /// <summary>
        /// Calculates the strait line distance between two geographic coordinates.
        /// </summary>
        /// <param name="aLatitude">
        /// The latitude of the first geographic coordinate.
        /// </param>
        /// <param name="aLongitude">
        /// The longitude of the first geographic coordinate.
        /// </param>
        /// <param name="bLatitude">
        /// The latitude of the second geographic coordinate.
        /// </param>
        /// <param name="bLongitude">
        /// The longitude of the second geographic coordinate.
        /// </param>
        /// <returns>
        /// The strait line distance between the two points.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method does not take into account differences in altitude.
        /// </para>
        /// <para>
        /// <b>IMPORTANT:</b> This method is not accurate over long distances.
        /// However, it is sufficient for scales used in most AR applications
        /// where distance is limited by human sight.
        /// </para>
        /// </remarks>
        static public float DistanceBetween(float aLatitude, float aLongitude, float bLatitude, float bLongitude)
        {
            float r = 6371f;
            float dLat = (bLatitude - aLatitude).ToRadian();
            float dLon = (bLongitude - aLongitude).ToRadian();
            float a = Mathf.Sin(dLat / 2f) * Mathf.Sin(dLat / 2f) +
                      Mathf.Cos(aLatitude.ToRadian()) * Mathf.Cos(bLatitude.ToRadian()) *
                      Mathf.Sin(dLon / 2f) * Mathf.Sin(dLon / 2f);
            float c = 2 * Mathf.Asin(Mathf.Min(1, Mathf.Sqrt(a)));
            float d = r * c;
            return d * 1000f;
        }

        /// <summary>
        /// Calculates the 3D distance, in meters, between the two coordinates.
        /// </summary>
        /// <param name="aLatitude">
        /// The latitude of the first geographic coordinate.
        /// </param>
        /// <param name="aLongitude">
        /// The longitude of the first geographic coordinate.
        /// </param>
        /// <param name="aAltitude">
        /// The altitude of the first geographic coordinate.
        /// </param>
        /// <param name="bLatitude">
        /// The latitude of the second geographic coordinate.
        /// </param>
        /// <param name="bLongitude">
        /// The longitude of the second geographic coordinate.
        /// </param>
        /// <param name="bAltitude">
        /// The altitude of the second geographic coordinate.
        /// </param>
        /// <returns>
        /// The 3D distance, in meters, between the two coordinates.
        /// </returns>
        /// <remarks>
        /// <para>
        /// In the <see cref="Vector3"/> returned, X represents longitude,
        /// Z represents latitude and Y represents altitude. The Z+ axis in
        /// this configuration aligns toward the North Pole.
        /// </para>
        /// <para>
        /// <b>IMPORTANT:</b> This method is not accurate over long distances.
        /// However, it is sufficient for scales used in most AR applications
        /// where distance is limited by human sight.
        /// </para>
        /// </remarks>
        static public Vector3 DistanceBetween3D(float aLatitude, float aLongitude, float aAltitude, float bLatitude, float bLongitude, float bAltitude)
        {
            // Use same longitude on both points to calculate latitude distance
            // and use same latitude on both points to calculate longitude
            // distance.
            float latitudeMeters = DistanceBetween(aLatitude, aLongitude, bLatitude, aLongitude);
            float longitudeMeters = DistanceBetween(aLatitude, aLongitude, aLatitude, bLongitude);

            // Invert the distance sign if necessary to account for direction
            if (aLatitude < bLatitude)
            {
                latitudeMeters *= -1;
            }
            if (aLongitude > bLongitude)
            {
                longitudeMeters *= -1;
            }

            // Calculate altitude difference
            float altitudeMeters = (bAltitude - aAltitude);

            // Return as Vector3
            return new Vector3(longitudeMeters, altitudeMeters, latitudeMeters);
        }

        /// <summary>
        /// Calculates the 3D distance, in meters, between the two locations.
        /// </summary>
        /// <param name="here">
        /// The first geographic location.
        /// </param>
        /// <param name="bLatitude">
        /// The latitude of the second geographic coordinate.
        /// </param>
        /// <param name="bLongitude">
        /// The longitude of the second geographic coordinate.
        /// </param>
        /// <param name="bAltitude">
        /// The altitude of the second geographic coordinate.
        /// </param>
        /// <returns>
        /// The 3D distance, in meters, between the two coordinates.
        /// </returns>
        /// <remarks>
        /// <para>
        /// In the <see cref="Vector3"/> returned, X represents longitude,
        /// Z represents latitude and Y represents altitude. The Z+ axis in
        /// this configuration aligns toward the North Pole.
        /// </para>
        /// <para>
        /// <b>IMPORTANT:</b> This method is not accurate over long distances.
        /// However, it is sufficient for scales used in most AR applications
        /// where distance is limited by human sight.
        /// </para>
        /// </remarks>
        static public Vector3 DistanceTo(this LocationInfo here, float bLatitude, float bLongitude, float bAltitude)
        {
            return DistanceBetween3D(
                       aLatitude: here.latitude,
                       aLongitude: here.longitude,
                       aAltitude: here.altitude,

                       bLatitude: bLatitude,
                       bLongitude: bLongitude,
                       bAltitude: bAltitude);
        }

        /// <summary>
        /// Calculates the 3D distance, in meters, between the two locations.
        /// </summary>
        /// <param name="here">
        /// The first geographic location.
        /// </param>
        /// <param name="there">
        /// The second geographic location.
        /// </param>
        /// <returns>
        /// The 3D distance, in meters, between the two coordinates.
        /// </returns>
        /// <remarks>
        /// <para>
        /// In the <see cref="Vector3"/> returned, X represents longitude,
        /// Z represents latitude and Y represents altitude. The Z+ axis in
        /// this configuration aligns toward the North Pole.
        /// </para>
        /// <para>
        /// <b>IMPORTANT:</b> This method is not accurate over long distances.
        /// However, it is sufficient for scales used in most AR applications
        /// where distance is limited by human sight.
        /// </para>
        /// </remarks>
        static public Vector3 DistanceTo(this LocationInfo here, LocationInfo there)
        {
            return DistanceBetween3D(
                       aLatitude: here.latitude,
                       aLongitude: here.longitude,
                       aAltitude: here.altitude,

                       bLatitude: there.latitude,
                       bLongitude: there.longitude,
                       bAltitude: there.altitude);
        }

        /// <summary>
        /// Converts a degree to a radian.
        /// </summary>
        /// <param name="val">
        /// The degree to convert.
        /// </param>
        /// <returns>
        /// The resulting radian.
        /// </returns>
        static public float ToRadian(this float val)
        {
            return Mathf.Deg2Rad * val;
        }

        /// <summary>
        /// Converts the <see cref="GeoCoordinate"/> to a properly formatted WGS84 string.
        /// </summary>
        /// <param name="location">
        /// The location to convert.
        /// </param>
        /// <returns>
        /// The WGS84 string.
        /// </returns>
        static public string ToWGS84String(this LocationInfo location)
        {
            return string.Format("{0}, {1}", location.latitude, location.longitude);
        }
        #endregion // Public Methods
    }
}
