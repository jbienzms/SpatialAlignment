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
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    public interface ISpatialAlignmentSystem : IMixedRealityEventSystem
    {
        /// <summary>
        /// Gets the parent object to which all spatial alignment <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>s are to be parented.
        /// </summary>
        GameObject SpatialAlignmentObjectParent { get; }

        /// <summary>
        /// Creates the a parent, that is a child of the Spatial Alignment System parent so that the scene hierarchy does not get overly cluttered.
        /// </summary>
        /// <returns>
        /// The <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see> to which spatial alignment objects will be parented.
        /// </returns>
        /// <remarks>
        /// This method is to be called by implementations of the <see cref="ISpatialCoordinateObserver"/> interface, not by application code. It
        /// is used to enable observations to be grouped by observer.
        /// </remarks>
        GameObject CreateSpatialAlignmentObservationParent(string name);

        /// <summary>
        /// Generates a new source identifier for an <see cref="ISpatialCoordinateObserver"/> implementation.
        /// </summary>
        /// <returns>The source identifier to be used by the <see cref="ISpatialCoordinateObserver"/> implementation.</returns>
        /// <remarks>
        /// This method is to be called by implementations of the <see cref="ISpatialCoordinateObserver"/> interface, not by application code.
        /// </remarks>
        uint GenerateNewSourceId();

        /// <summary>
        /// Typed representation of the ConfigurationProfile property.
        /// </summary>
        SpatialAlignmentSystemProfile SpatialAlignmentSystemProfile { get; }

        /// <summary>
        /// Starts / restarts all spatial observers of the specified type.
        /// </summary>
        void ResumeObservers();

        /// <summary>
        /// Starts / restarts all spatial observers of the specified type.
        /// </summary>
        /// <typeparam name="T">The desired spatial awareness observer type (ex: <see cref="NativeAnchorObserver"/>)</typeparam>
        void ResumeObservers<T>() where T : ISpatialCoordinateObserver;

        /// <summary>
        /// Starts / restarts the spatial observer registered under the specified name matching the specified type.
        /// </summary>
        /// <typeparam name="T">The desired spatial awareness observer type (ex: <see cref="NativeAnchorObserver"/>)</typeparam>
        /// <param name="name">The friendly name of the observer.</param>
        void ResumeObserver<T>(string name) where T : ISpatialCoordinateObserver;

        /// <summary>
        /// Stops / pauses all spatial observers.
        /// </summary>
        void SuspendObservers();

        /// <summary>
        /// Stops / pauses all spatial observers of the specified type.
        /// </summary>
        void SuspendObservers<T>() where T : ISpatialCoordinateObserver;

        /// <summary>
        /// Stops / pauses the spatial observer registered under the specified name matching the specified type.
        /// </summary>
        /// <typeparam name="T">The desired spatial awareness observer type (ex: <see cref="IMixedRealityNativeAnchorObserver"/>)</typeparam>
        /// <param name="name">The friendly name of the observer.</param>
        void SuspendObserver<T>(string name) where T : ISpatialCoordinateObserver;

        /// <summary>
        /// Clears all registered observers' observations.
        /// </summary>
        void ClearObservations();

        /// <summary>
        /// Clears the observations of the specified observer.
        /// </summary>
        /// <typeparam name="T">The observer type.</typeparam>
        /// <param name="name">The name of the observer.</param>
        void ClearObservations<T>(string name = null) where T : ISpatialCoordinateObserver;

        /// <summary>
        /// <see cref="ISpatialCoordinateObserver"/>'s should call this method to indicate a coordinate has been added.
        /// </summary>
        /// <param name="observer">The observer raising the event.</param>
        /// <param name="coordinate">The coordinate <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>.</param>
        /// <remarks>
        /// This method is to be called by implementations of the <see cref="ISpatialCoordinateObserver"/> interface, not by application code.
        /// </remarks>
        void RaiseCoordinateAdded(ISpatialCoordinateObserver observer, ISpatialCoordinate coordinate);

        /// <summary>
        /// <see cref="ISpatialCoordinateObserver"/>'s should call this method to indicate an existing coordinate has been updated.
        /// </summary>
        /// <param name="observer">The observer raising the event.</param>
        /// <param name="coordinate">The coordinate <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>.</param>
        /// <remarks>
        /// This method is to be called by implementations of the <see cref="ISpatialCoordinateObserver"/> interface, not by application code.
        /// </remarks>
        void RaiseCoordinateUpdated(ISpatialCoordinateObserver observer, ISpatialCoordinate coordinate);

        /// <summary>
        /// <see cref="ISpatialCoordinateObserver"/>'s should call this method to indicate an existing coordinate has been removed.
        /// </summary>
        /// <param name="observer">The observer raising the event.</param>
        /// <param name="coordinateId">Value identifying the coordinate.</param>
        /// <remarks>
        /// This method is to be called by implementations of the <see cref="ISpatialCoordinateObserver"/> interface, not by application code.
        /// </remarks>
        void RaiseCoordinateRemoved(ISpatialCoordinateObserver observer, string coordinateId);
    }
}
