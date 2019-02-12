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
    public class GeoAlignment : AlignmentStrategy
    {
        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Tooltip("The geodetic altitude the frame will be aligned to.")]
        private float altitude;

        [DataMember]
        [SerializeField]
        [Tooltip("The reference source used when converting between global and application 3D space.")]
        private GeoReference geoReference;

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

        #region Internal Methods
        /// <summary>
        /// Subscribe to reference events.
        /// </summary>
        private void SubscribeReference()
        {
            // Subscribe to reference events
            geoReference.ReferenceDataChanged += GeoReference_ReferenceUpdated;
        }

        /// <summary>
        /// Unsubscribe from reference events.
        /// </summary>
        private void UnsubscribeReference()
        {
            // Subscribe to reference events
            geoReference.ReferenceDataChanged -= GeoReference_ReferenceUpdated;
        }

        /// <summary>
        /// Updates the transform based on the current coordinates related to
        /// the <see cref="GeoReference"/>
        /// </summary>
        protected void UpdateTransform()
        {
            // Attempt to get data from the reference
            GeoReferenceData geoData = geoReference?.ReferenceData;

            // If we have no reference or data, we're in inhibited state
            if (geoData == null)
            {
                State = AlignmentState.Inhibited;
                Debug.LogWarning($"{nameof(GeoAlignment)} {name}: {nameof(GeoReference)} data unavailable - Inhibited.");
                return;
            }

            // Get the distance between us and the reference point
            Vector3 offset;
            if (relativeAltitude)
            {
                offset = geoData.GeoPosition.DistanceTo(latitude, longitude, geoData.GeoPosition.altitude + altitude);
            }
            else
            {
                offset = geoData.GeoPosition.DistanceTo(latitude, longitude, altitude);
            }

            // Calculate our new position based on the reference position plus offset
            Vector3 pos = geoData.LocalPosition + offset;

            Debug.Log(
                $"GPS Lat: {geoData.GeoPosition.latitude}, Lon: {geoData.GeoPosition.longitude}, Alt: {geoData.GeoPosition.altitude}\r\n" +
                $"Target Lat: {this.latitude}, Lon: {this.longitude}, Alt: {this.altitude}\r\n" +
                $"GPS Offset x: {offset.x}, y: {offset.y}, z: {offset.z}\r\n" +
                $"Pos x: {pos.x}, y: {pos.y}, z: {pos.z}\r\n" +
                $"");

            // Update our transform position
            transform.position = pos;

            // If north is not 0, we need to rotate around the reference point
            if (geoData.NorthHeading != 0f)
            {
                transform.RotateAround(geoData.LocalPosition, Vector3.up, -geoData.NorthHeading);
            }

            // TODO: Account for north if the app was not started facing north
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers
        private void GeoReference_ReferenceUpdated(object sender, EventArgs e)
        {
            // Get the data
            GeoReferenceData geoData = geoReference.ReferenceData;

            // Update our stats
            State = AlignmentState.Tracking;
            float h = geoData.HorizontalAccuracy;
            float v = geoData.VerticalAccuracy;
            Accuracy = new Vector3(h, v, h);

            // Reference has updated. Time to update Transform, but must do it
            // on the UI thread.
            UnityDispatcher.InvokeOnAppThread(UpdateTransform);
        }
        #endregion // Overrides / Event Handlers

        #region Unity Overrides
        /// <inheritdoc />
        protected override void OnDisable()
        {
            // If we have a reference, stop tracking
            if (geoReference != null)
            {
                UnsubscribeReference();
                State = AlignmentState.Inhibited;
            }

            // Pass on to base
            base.OnDisable();
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            // Pass to base first
            base.OnEnable();

            // If we have a reference, start tracking
            if (geoReference != null)
            {
                SubscribeReference();
            }

            // Update
            UpdateTransform();
        }
        #endregion // Unity Overrides

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
        /// Gets or sets the reference source used when converting between
        /// global and application 3D space.
        /// </summary>
        public GeoReference GeoReference
        {
            get
            {
                return geoReference;
            }
            set
            {
                // Is it changing?
                if (geoReference != value)
                {
                    // If old reference, unsubscribe
                    if (geoReference != null)
                    {
                        UnsubscribeReference();
                    }

                    // Store
                    geoReference = value;

                    // If new reference, subscribe and update
                    if (geoReference != null)
                    {
                        SubscribeReference();
                    }

                    // Update
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