// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    public interface IMixedRealitySpatialCoordinateObserver : IMixedRealityDataProvider, IMixedRealityEventSource
    {
        /// <summary>
        /// Indicates the developer's intended startup behavior.
        /// </summary>
        AutoStartBehavior StartupBehavior { get; set; }

        /// <summary>
        /// Is the observer running (actively accumulating spatial corrdinates)?
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets or sets the extents( 1/2 size) of the volume, in meters per axis, from which individual observations will be made.
        /// </summary>
        /// <remarks>
        /// When used when <see cref="ObserverVolumeType"/> is set to <see cref="Microsoft.MixedReality.Toolkit.Utilities.VolumeType.Sphere"/> the X  value of the extents will be
        /// used as the radius.
        /// </remarks>
        Vector3 ObservationExtents { get; set; }

        /// <summary>
        /// Gets or sets the orientation of the volume.
        /// </summary>
        Quaternion ObserverRotation { get; set; }

        /// <summary>
        /// Gets or sets the origin of the observer.
        /// </summary>
        /// <remarks>
        /// Moving the observer origin allows the spatial awareness system to locate and discard meshes as the user
        /// navigates the environment.
        /// </remarks>
        Vector3 ObserverOrigin { get; set; }

        /// <summary>
        /// Gets or sets the frequency, in seconds, at which the spatial observer should update.
        /// </summary>
        float UpdateInterval { get; set; }

        /// <summary>
        /// Start | resume the observer.
        /// </summary>
        void Resume();

        /// <summary>
        /// Stop | pause the observer
        /// </summary>
        void Suspend();

        /// <summary>
        /// Clears the observer's collection of observations.
        /// </summary>
        /// <remarks>
        /// If the observer is currently running, calling ClearObservations will suspend it.
        /// </remarks>
        void ClearObservations();
    }
}