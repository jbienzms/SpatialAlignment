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

using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    public interface ISpatialCoordinateObserver : IMixedRealityDataProvider, IMixedRealityEventSource
    {
        /// <summary>
        /// Indicates the developer's intended startup behavior.
        /// </summary>
        AutoStartBehavior StartupBehavior { get; set; }

        /// <summary>
        /// Is the observer running (actively accumulating spatial coordinates)?
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