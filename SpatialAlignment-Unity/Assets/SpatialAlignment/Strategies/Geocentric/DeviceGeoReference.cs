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
using System.Threading;
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
    public class DeviceGeoReference : GeoReference
    {
        #region Member Variables
        private double lastTimestamp;						// The time stamp of the last location report
        private bool restartForDesiredAccuracy = true;      // Whether changes to DesiredAccuracy require a restart
        private bool restartForUpdateDistance = true;       // Whether changes to UpdateDistance require a restart
        private Task startTrackingTask;						// The Task that is used to start tracking
        #endregion // Member Variables

        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Tooltip("Desired accuracy in meters. The default is 10.")]
        [Min(0.1f)]
        private float desiredAccuracy = 10f;

        [DataMember]
        [SerializeField]
        [Tooltip("The amount of (in seconds) time to wait for tracking to start. -1 means forever and the default is 20.")]
        [Min(-1.0f)]
        private float startTrackingTimeout = 20f;

        [DataMember]
        [SerializeField]
        [Tooltip("Update distance in meters. The default is 10.")]
        private float updateDistance = 1.5f;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Actual implementation to start tracking.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the
        /// operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        private async Task InnerStartTrackingAsync(CancellationToken cancellationToken)
        {
            #if UNITY_EDITOR
            // Wait until Unity connects to the Unity Remote
            while (!UnityEditor.EditorApplication.isRemoteConnected)
            {
                await Task.Delay(500, cancellationToken);
            }
            #endif

            // First, check if user has location service enabled
            //if (!Input.location.isEnabledByUser)
            //{
            //    throw new UnauthorizedAccessException("User has blocked location access.");
            //}

            // Start service before querying location
            Input.location.Start(desiredAccuracy, updateDistance);

            // Wait until service initializes
            while (Input.location.status == LocationServiceStatus.Initializing)
            {
                await Task.Delay(500, cancellationToken);
            }

            // Connection has failed
            if (Input.location.status != LocationServiceStatus.Running)
            {
                throw new UnauthorizedAccessException("Location services could not be started.");
            }

            // Tracking!
            IsTracking = true;
        }

        /// <summary>
        /// Restarts tracking to apply changes.
        /// </summary>
        private async void RestartTracking()
        {
            if (!IsTracking)
            {
                Debug.LogWarning($"{nameof(DeviceGeoReference)} {nameof(RestartTracking)} called but device is not tracking.");
                return;
            }

            // Stop tracking
            StopTracking();

            // Try to start tracking again
            TryStartTracking();
        }

        /// <summary>
        /// Attempts to start tracking and logs if there's a failure.
        /// </summary>
        private async void TryStartTracking()
        {
            try
            {
                await StartTrackingAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Could not start tracking: {ex.Message}");
            }
        }
        #endregion // Internal Methods

        #region Unity Overrides
        /// <inheritdoc />
        protected override void Start()
        {
            // Call base first
            base.Start();

            // Auto start?
            if (AutoStartTracking)
            {
                TryStartTracking();
            }
        }

        /// <inheritdoc />
        protected override void Update()
        {
            // TODO: Move this to a separate thread so it's not taking render cycles

            // Tracking?
            if ((IsTracking) && (Input.location.status == LocationServiceStatus.Running))
            {
                // Get last data
                LocationInfo location = Input.location.lastData;

                // Was there an update?
                if (location.timestamp > lastTimestamp)
                {
                    // Update time stamp
                    lastTimestamp = location.timestamp;

                    // Update the reference
                    UpdateReference(location);
                }
            }

            // Pass on to base
            base.Update();
        }
        #endregion // Unity Overrides

        #region Public Methods
        /// <summary>
        /// Starts tracking.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the
        /// operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public Task StartTrackingAsync(CancellationToken cancellationToken)
        {
            // Make sure we're not starting it again
            if ((IsTracking) || ((startTrackingTask != null) && (!startTrackingTask.IsCompleted)))
            {
                throw new InvalidOperationException("Tracking has already been started.");
            }

            // Start and store the task
            startTrackingTask = InnerStartTrackingAsync(cancellationToken);

            // Return the task
            return startTrackingTask;
        }

        /// <summary>
        /// Starts tracking.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        public Task StartTrackingAsync()
        {
            // Call cancellation overload, optionally using timeout
            if (startTrackingTimeout > 0)
            {
                return StartTrackingAsync(new CancellationTokenSource(TimeSpan.FromSeconds(startTrackingTimeout)).Token);
            }
            else
            {
                return StartTrackingAsync(CancellationToken.None);
            }
        }

        /// <summary>
        /// Stops tracking.
        /// </summary>
        public void StopTracking()
        {
            // Make sure not tracking
            if (!IsTracking)
            {
                throw new InvalidOperationException($"{nameof(StopTracking)} called but device is not tracking.");
            }

            // Stop tracking
            Input.location.Stop();

            // No longer tracking
            IsTracking = false;
        }
        #endregion // Public Methods

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
                    if ((IsTracking) && (restartForDesiredAccuracy))
                    {
                        RestartTracking();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the amount of time (in seconds) to wait for tracking
        /// to start. -1 means wait forever. The default is 20.
        /// </summary>
        /// <remarks>
        /// When the device is accessed for the first time, a system dialog
        /// may appear prompting the user for permission. If the user does not
        /// grant permission within this timeout the starting task will fail
        /// with a <see cref="TaskCanceledException"/>.
        /// </remarks>
        public float StartTrackingTimeout { get => startTrackingTimeout; set => startTrackingTimeout = value; }

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
                        RestartTracking();
                    }
                }
            }
        }
        #endregion // Public Properties
    }
}
