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
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.SpatialAlignment.Geocentric
{
    /// <summary>
    /// A strategy that aligns the frame to a known Geocentric (GPS) location.
    /// </summary>
    [DataContract]
    public class GeoAlignment : HeadingAlignment
    {
        #region Member Variables
        Vector3 lastPosition;			// The last calculated position in local space.
        #endregion // Member Variables

        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Tooltip("The geodetic altitude the frame will be aligned to.")]
        private float altitude;

        [DataMember]
        [SerializeField]
        [Tooltip("The geodetic latitude the frame will be aligned to.")]
        private float latitude;

        [DataMember]
        [SerializeField]
        [Tooltip("The geodetic longitude the frame will be aligned to.")]
        private float longitude;

        [DataMember]
        [SerializeField]
        [Tooltip("Whether altitude should be considered relative to the reference altitude.")]
        private bool relativeAltitude;
        #endregion // Unity Inspector Variables

        #region Overrides / Event Handlers
        /// <inheritdoc />
        protected override void ApplyHeading(HeadingData heading)
        {
            // Only attempt to apply if a heading is available.
            // Not having a heading doesn't necessarily mean
            // we're inhibited
            if (heading != null)
            {
                // We also need location in order to handle heading changes
                LocationData location = GeoReference.Location;

                // If the GeoReference has a NorthHeading of anything other than
                // 0, we need to rotate around the references local position as a
                // pivot point. This step is what accounts for the device not
                // facing North on application launch.
                if ((heading.NorthHeading != 0f) && (location != null))
                {
                    transform.position = lastPosition;
                    transform.RotateAround(location.LocalPosition, Vector3.up, heading.NorthHeading);
                }
            }
            // Finally, pass on to base to apply local rotation.
            base.ApplyHeading(heading);
        }

        /// <inheritdoc />
        protected override void ApplyLocation(LocationData location)
        {
            // Pass to base first (HeadingAlignment doesn't do anything with location)
            base.ApplyLocation(location);

            // If we have no reference or data, we're in inhibited state
            if (location == null)
            {
                State = AlignmentState.Inhibited;
                Debug.LogWarning($"{nameof(GeoAlignment)} {name}: {nameof(GeoReference)} data unavailable - Inhibited.");
                return;
            }

            // Get the distance between us and the reference point
            Vector3 offset;
            if (relativeAltitude)
            {
                offset = location.GeoPosition.DistanceTo(latitude, longitude, location.GeoPosition.altitude + altitude);
            }
            else
            {
                offset = location.GeoPosition.DistanceTo(latitude, longitude, altitude);
            }

            // Calculate our new position based on the reference position plus offset
            lastPosition = location.LocalPosition + offset;

            //Debug.Log(
            //    $"GPS Lat: {location.GeoPosition.latitude}, Lon: {location.GeoPosition.longitude}, Alt: {location.GeoPosition.altitude}\r\n" +
            //    $"Target Lat: {this.latitude}, Lon: {this.longitude}, Alt: {this.altitude}\r\n" +
            //    $"GPS Offset x: {offset.x}, y: {offset.y}, z: {offset.z}\r\n" +
            //    $"Pos x: {lastPosition.x}, y: {lastPosition.y}, z: {lastPosition.z}\r\n" +
            //    $"");

            // Update our transform position
            transform.position = lastPosition;
        }
        #endregion // Overrides / Event Handlers

        #region Public Properties
        /// <summary>
        /// Gets or sets the
        /// <see cref="https://en.wikipedia.org/wiki/World_Geodetic_System">geodetic</see>
        /// altitude the frame will be aligned to.
        /// </summary>
        /// <remarks>
        /// Changing this value will cause the transform to be updated.
        /// </remarks>
        public float Altitude
        {
            get
            {
                return altitude;
            }
            set
            {
                // Ensure changing
                if (altitude != value)
                {
                    altitude = value;
                    UpdateTransform();
                }
            }
        }

        /// <summary>
        /// Gets or sets the
        /// <see cref="https://en.wikipedia.org/wiki/World_Geodetic_System">geodetic</see>
        /// latitude the frame will be aligned to.
        /// </summary>
        /// <remarks>
        /// Changing this value will cause the transform to be updated.
        /// </remarks>
        public float Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                // Ensure changing
                if (latitude != value)
                {
                    latitude = value;
                    UpdateTransform();
                }
            }
        }

        /// <summary>
        /// Gets or sets the
        /// <see cref="https://en.wikipedia.org/wiki/World_Geodetic_System">geodetic</see>
        /// longitude the frame will be aligned to.
        /// </summary>
        /// <remarks>
        /// Changing this value will cause the transform to be updated.
        /// </remarks>
        public float Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                // Ensure changing
                if (longitude != value)
                {
                    longitude = value;
                    UpdateTransform();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether <see cref="Altitude"/>
        /// should be considered relative to <see cref="GeoReference"/>
        /// altitude.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the actual altitude is not known, this property can be
        /// used to align the frame at the same altitude as the
        /// <see cref="GeoReference">reference</see> position (normally the
        /// user). When this property is set to <c>true</c>,
        /// <see cref="Altitude"/> is used as an offset above or below the
        /// reference point.
        /// </para>
        /// <para>
        /// Changing this value will cause the transform to be updated.
        /// </para>
        /// </remarks>
        public bool RelativeAltitude
        {
            get
            {
                return relativeAltitude;
            }
            set
            {
                // Ensure changing
                if (relativeAltitude != value)
                {
                    relativeAltitude = value;
                    UpdateTransform();
                }
            }
        }
        #endregion // Public Properties
    }
}