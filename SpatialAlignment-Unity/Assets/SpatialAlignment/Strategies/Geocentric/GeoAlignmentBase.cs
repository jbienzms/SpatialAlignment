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
    /// A base class for a strategy that aligns based on a <see cref="GeoReference"/>.
    /// </summary>
    [DataContract]
    public abstract class GeoAlignmentBase : AlignmentStrategy
    {
        #region Unity Inspector Variables
        [DataMember]
        [SerializeField]
        [Tooltip("The reference source used to calculate position and rotation.")]
        private GeoReference geoReference;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Subscribe to reference events.
        /// </summary>
        private void SubscribeReference()
        {
            // Subscribe to reference events
            geoReference.HeadingChanged += GeoReference_HeadingChanged;
            geoReference.LocationChanged += GeoReference_LocationChanged;
        }

        /// <summary>
        /// Unsubscribe from reference events.
        /// </summary>
        private void UnsubscribeReference()
        {
            // Subscribe to reference events
            geoReference.HeadingChanged -= GeoReference_HeadingChanged;
            geoReference.LocationChanged -= GeoReference_LocationChanged;
        }

        /// <summary>
        /// Updates the transform based on the current coordinates related to
        /// the <see cref="GeoReference"/>
        /// </summary>
        protected void UpdateTransform()
        {
            // Attempt to get data from the reference
            HeadingData heading = geoReference?.Heading;
            LocationData location = geoReference?.Location;

            // Call overload
            ApplyHeading(heading);
            ApplyLocation(location);
        }
        #endregion // Internal Methods

        #region Overridables / Event Triggers
        /// <summary>
        /// Updates the transform to match the current heading of the
        /// the <see cref="GeoReference"/>.
        /// </summary>
        /// <param name="heading">
        /// The <see cref="HeadingData"/> used to calculate the transform.
        /// </param>
        protected abstract void ApplyHeading(HeadingData heading);

        /// <summary>
        /// Updates the transform to match the current location of the
        /// the <see cref="GeoReference"/>.
        /// </summary>
        /// <param name="location">
        /// The <see cref="LocationData"/> used to calculate the transform.
        /// </param>
        protected abstract void ApplyLocation(LocationData location);
        #endregion // Overridables / Event Triggers

        #region Overrides / Event Handlers
        private void GeoReference_HeadingChanged(object sender, EventArgs e)
        {
            // Get the data
            HeadingData heading = geoReference.Heading;

            // Update our state to show we're tracking
            State = AlignmentState.Tracking;

            // Reference has updated. Time to update Transform, but must do it
            // on the UI thread.
            UnityDispatcher.InvokeOnAppThread(() => ApplyHeading(heading));
        }

        private void GeoReference_LocationChanged(object sender, EventArgs e)
        {
            // Get the data
            LocationData location = geoReference.Location;

            // Update our state to show we're tracking
            State = AlignmentState.Tracking;

            // Update our accuracy values
            float h = location.HorizontalAccuracy;
            float v = location.VerticalAccuracy;
            Accuracy = new Vector3(h, v, h);

            // Reference has updated. Time to update Transform, but must do it
            // on the UI thread.
            UnityDispatcher.InvokeOnAppThread(() => ApplyLocation(location));
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
        #endregion // Public Properties
    }
}