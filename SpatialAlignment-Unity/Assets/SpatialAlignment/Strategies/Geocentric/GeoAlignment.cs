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

using Microsoft.SpatialAlignment.Persistence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;

namespace Microsoft.SpatialAlignment
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
        [Tooltip("The source that will serve as a reference point when converting from geocentric to Unity space.")]
        private IGeoReference geoReference;

        [DataMember]
        [SerializeField]
        [Tooltip("The geocentric latitude the frame will align to.")]
        private float latitude;
        #endregion // Unity Inspector Variables

        #region Internal Methods
        /// <summary>
        /// Subscribe to reference events.
        /// </summary>
        private void SubscribeReference()
        {
            // Subscribe to reference events
            geoReference.ReferenceUpdated += GeoReference_ReferenceUpdated;
        }

        /// <summary>
        /// Unsubscribe from reference events.
        /// </summary>
        private void UnsubscribeReference()
        {
            // Subscribe to reference events
            geoReference.ReferenceUpdated -= GeoReference_ReferenceUpdated;
        }

        /// <summary>
        /// Updates the transform based on the current coordinates related to
        /// the <see cref="GeoReference"/>
        /// </summary>
        protected void UpdateTransform()
        {
            // If we have no reference, we're in inhibited state
            if (geoReference == null)
            {
                State = AlignmentState.Inhibited;
                return;
            }

            // TODO: Convert our geocentric values to ECEF values
            // TODO: Set our transform based on the reference source
        }
        #endregion // Internal Methods

        #region Overrides / Event Handlers
        private void GeoReference_ReferenceUpdated(object sender, EventArgs e)
        {
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
            }

            // Pass on to base
            base.OnDisable();
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            // Pass to base first
            base.OnEnable();

            // If we have a reference, start tracking and update
            if (geoReference != null)
            {
                SubscribeReference();
                UpdateTransform();
            }
        }
        #endregion // Unity Overrides

        #region Public Properties
        /// <summary>
        /// Gets or sets the ID of the anchor to load.
        /// </summary>
        public IGeoReference GeoReference
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
                        UpdateTransform();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the geocentric latitude the frame will align to.
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
        #endregion // Public Properties
    }
}