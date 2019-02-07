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
    /// This enum is reserved for future use.
    /// </summary>
    public enum GeoDeviceType
    {
        /// <summary>
        /// Location data is provided by the platform.
        /// </summary>
        /// <remarks>
        /// In Unity this is provided by <see cref="Input.location"/>.
        /// </remarks>
        Platform,

        /// <summary>
        /// Location data is provided by a NMEA compatible GPS device
        /// connected via serial interface.
        /// </summary>
        /// <remarks>
        /// The serial interface may be emulated, such as a virtual USB
        /// COM port or RfComm over Bluetooth.
        /// </remarks>
        SerialNMEA,
    }

    /// <summary>
    /// An <see cref="IGeoReference"/> source that provides updates from a
    /// GPS device or on-device location services.
    /// </summary>
    [DataContract]
    public class DeviceGeoReference : MonoBehaviour, IGeoReference
    {
        #region Member Variables
        private bool isTracking = false;                    // Whether tracking has been started
        private GeoReferenceData referenceData;				// The last reported reference data
        private bool restartForDesiredAccuracy = true;      // Whether changes to DesiredAccuracy require a restart
        private bool restartForUpdateDistance = true;		// Whether changes to UpdateDistance require a restart
        #endregion // Member Variables

        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Tooltip("Desired accuracy in meters. The default is 10.")]
        [Min(0.1f)]
        private float desiredAccuracy = 10f;

        [DataMember]
        [SerializeField]
        [Tooltip("Whether tracking should begin automatically.")]
        private bool trackOnStart = true;

        [DataMember]
        [SerializeField]
        [Tooltip("Update distance in meters. The default is 10.")]
        private float updateDistance = 1.5f;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Reinitializes the device if it is currently tracking.
        /// </summary>
        private void RestartDevice()
        {
            if (!isTracking)
            {
                Debug.LogWarning($"{nameof(DeviceGeoReference)} {nameof(RestartDevice)} called but device is not tracking.");
                return;
            }

        }
        #endregion // Internal Methods

        /// <summary>
        /// Called whenever the value of the <see cref="ReferenceData"/> property
        /// has changed.
        /// </summary>
        protected virtual void OnReferenceDataChanged()
        {
            ReferenceDataChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Public Properties
        /// <summary>
        /// Gets or sets the desired accuracy in meters. The default is 10.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Higher values (e.g. > 500) may allow some platforms to provide
        /// location updates without turning on a GPS chip. This reduces power
        /// consumption and increased battery life. Values smaller than 10 are
        /// acceptable, but position updates will likely never be more accurate
        /// than 10 meters due to the limitations of GPS itself.
        /// </para>
        /// <para>
        /// Some platforms (especially Unity) require the device to be
        /// restarted for changes to take effect. When necessary, changes to
        /// this property may restart the GPS device.
        /// </para>
        /// </remarks>
        public float DesiredAccuracy
        {
            get
            {
                return desiredAccuracy;
            }
            set
            {
                // Changing?
                if (desiredAccuracy != value)
                {
                    // Validate
                    if (value <= 0.0f) { throw new ArgumentOutOfRangeException(nameof(value)); }

                    // Store
                    desiredAccuracy = value;

                    // Restart required?
                    if ((isTracking) && (restartForDesiredAccuracy))
                    {
                        RestartDevice();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates if the device is currently tracking
        /// and providing updates.
        /// </summary>
        public bool IsTracking { get => isTracking; }

        /// <inheritdoc />
        public GeoReferenceData ReferenceData
        {
            get
            {
                return referenceData;
            }
            set
            {
                // Changing?
                if (referenceData != value)
                {
                    referenceData = value;
                    OnReferenceDataChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates if tracking should start
        /// automatically when the behavior starts.
        /// </summary>
        public bool TrackOnStart { get => trackOnStart; set => trackOnStart = value; }

        /// <summary>
        /// Gets or sets the distance the device must move before an updated is
        /// reported. The default is 10.
        /// </summary>
        /// <remarks>
        /// Some platforms (especially Unity) require the device to be
        /// restarted for changes to take effect. When necessary, changes to
        /// this property may restart the GPS device.
        /// </remarks>
        public float UpdateDistance
        {
            get
            {
                return updateDistance;
            }
            set
            {
                // Changing?
                if (updateDistance != value)
                {
                    // Validate
                    if (value <= 0.0f) { throw new ArgumentOutOfRangeException(nameof(value)); }

                    // Store
                    updateDistance = value;

                    // Restart required?
                    if ((IsTracking) && (restartForUpdateDistance))
                    {
                        RestartDevice();
                    }
                }
            }
        }
        #endregion // Public Properties

        #region Public Events
        /// <summary>
        /// Raised whenever the value of the <see cref="ReferenceData"/> property
        /// has changed.
        /// </summary>
        public event EventHandler ReferenceDataChanged;
        #endregion // Public Events

    }
}
