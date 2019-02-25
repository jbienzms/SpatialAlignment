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
    /// Base class to simplify developing an <see cref="IGeoReference"/>.
    /// </summary>
    [DataContract]
    public abstract class GeoReference : MonoBehaviour, IGeoReference
    {
        #region Member Variables
        private HeadingData heading;						// The last reported heading data
        private bool isTracking = false;                    // Whether tracking has been started
        private LocationData location;						// The last reported location data
        #endregion // Member Variables

        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Tooltip("Whether tracking should begin automatically.")]
        private bool autoStartTracking = true;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Updates the heading data to match the specified values.
        /// </summary>
        /// <param name="northHeading">
        /// The new north heading value.
        /// </param>
        /// <param name="northAccuracy">
        /// The new north accuracy value.
        /// </param>
        protected void UpdateHeading(float northHeading, float northAccuracy)
        {
            // Create new reference data
            HeadingData data = new HeadingData(
                northHeading: northHeading,
                northAccuracy: northAccuracy);

            // Update (and notify)
            Heading = data;
        }

        /// <summary>
        /// Updates the location data to match the specified values.
        /// </summary>
        /// <param name="location">
        /// The location info used in the update.
        /// </param>
        protected void UpdateLocation(LocationInfo location)
        {
            // Create new reference data
            LocationData data = new LocationData(
                geoPosition: location,
                localPosition: this.transform.position,
                horizontalAccuracy: location.horizontalAccuracy,
                verticalAccuracy: location.verticalAccuracy);

            // Update (and notify)
            Location = data;
        }
        #endregion // Internal Methods

        #region Overridables / Event Triggers
        /// <summary>
        /// Called whenever the value of the <see cref="Heading"/> property
        /// has changed.
        /// </summary>
        protected virtual void OnHeadingChanged()
        {
            HeadingChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called whenever the value of the <see cref="Location"/> property
        /// has changed.
        /// </summary>
        protected virtual void OnLocationChanged()
        {
            LocationChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion // Overridables / Event Triggers

        #region Unity Overrides
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        protected virtual void Awake() { }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        protected virtual void OnDestroy() { }

        /// <summary>
        /// Called each time the script is disabled.
        /// </summary>
        protected virtual void OnDisable() { }

        /// <summary>
        /// Called each time the script is enabled.
        /// </summary>
        protected virtual void OnEnable() { }

        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        protected virtual void Start() { }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        protected virtual void Update() { }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets a value that indicates if tracking should start
        /// automatically when the behavior starts.
        /// </summary>
        public bool AutoStartTracking { get => autoStartTracking; set => autoStartTracking = value; }

        /// <inheritdoc />
        public HeadingData Heading
        {
            get
            {
                return heading;
            }
            protected set
            {
                // Changing?
                if (heading != value)
                {
                    heading = value;
                    OnHeadingChanged();
                }
            }
        }

        /// <inheritdoc />
        public bool IsTracking { get => isTracking; protected set => isTracking = value; }

        /// <inheritdoc />
        public LocationData Location
        {
            get
            {
                return location;
            }
            protected set
            {
                // Changing?
                if (location != value)
                {
                    location = value;
                    OnLocationChanged();
                }
            }
        }
        #endregion // Public Properties

        #region Public Events
        /// <inheritdoc />
        public event EventHandler HeadingChanged;

        /// <inheritdoc />
        public event EventHandler LocationChanged;
        #endregion // Public Events

    }
}
