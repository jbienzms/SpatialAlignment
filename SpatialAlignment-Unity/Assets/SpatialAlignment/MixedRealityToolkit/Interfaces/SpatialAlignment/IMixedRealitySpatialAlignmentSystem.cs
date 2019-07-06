// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    public interface IMixedRealitySpatialAlignmentSystem : IMixedRealityEventSystem
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
        /// This method is to be called by implementations of the <see cref="IMixedRealitySpatialAlignmentObserver"/> interface, not by application code. It
        /// is used to enable observations to be grouped by observer.
        /// </remarks>
        GameObject CreateSpatialAlignmentObservationParent(string name);

        /// <summary>
        /// Generates a new source identifier for an <see cref="IMixedRealitySpatialCoordinateObserver"/> implementation.
        /// </summary>
        /// <returns>The source identifier to be used by the <see cref="IMixedRealitySpatialCoordinateObserver"/> implementation.</returns>
        /// <remarks>
        /// This method is to be called by implementations of the <see cref="IMixedRealitySpatialCoordinateObserver"/> interface, not by application code.
        /// </remarks>
        uint GenerateNewSourceId();

        /// <summary>
        /// Typed representation of the ConfigurationProfile property.
        /// </summary>
        MixedRealitySpatialAlignmentSystemProfile SpatialAlignmentSystemProfile { get; }

        /// <summary>
        /// Starts / restarts all spatial observers of the specified type.
        /// </summary>
        void ResumeObservers();

        /// <summary>
        /// Starts / restarts all spatial observers of the specified type.
        /// </summary>
        /// <typeparam name="T">The desired spatial awareness observer type (ex: <see cref="IMixedRealityNativeAnchorObserver"/>)</typeparam>
        void ResumeObservers<T>() where T : IMixedRealitySpatialCoordinateObserver;

        /// <summary>
        /// Starts / restarts the spatial observer registered under the specified name matching the specified type.
        /// </summary>
        /// <typeparam name="T">The desired spatial awareness observer type (ex: <see cref="IMixedRealityNativeAnchorObserver"/>)</typeparam>
        /// <param name="name">The friendly name of the observer.</param>
        void ResumeObserver<T>(string name) where T : IMixedRealitySpatialCoordinateObserver;

        /// <summary>
        /// Stops / pauses all spatial observers.
        /// </summary>
        void SuspendObservers();

        /// <summary>
        /// Stops / pauses all spatial observers of the specified type.
        /// </summary>
        void SuspendObservers<T>() where T : IMixedRealitySpatialCoordinateObserver;

        /// <summary>
        /// Stops / pauses the spatial observer registered under the specified name matching the specified type.
        /// </summary>
        /// <typeparam name="T">The desired spatial awareness observer type (ex: <see cref="IMixedRealityNativeAnchorObserver"/>)</typeparam>
        /// <param name="name">The friendly name of the observer.</param>
        void SuspendObserver<T>(string name) where T : IMixedRealitySpatialCoordinateObserver;

        /// <summary>
        /// Clears all registered observers' observations.
        /// </summary>
        void ClearObservations();

        /// <summary>
        /// Clears the observations of the specified observer.
        /// </summary>
        /// <typeparam name="T">The observer type.</typeparam>
        /// <param name="name">The name of the observer.</param>
        void ClearObservations<T>(string name = null) where T : IMixedRealitySpatialCoordinateObserver;

        /// <summary>
        /// <see cref="IMixedRealityNativeAnchorObserver"/>'s should call this method to indicate a coordinate has been added.
        /// </summary>
        /// <param name="observer">The observer raising the event.</param>
        /// <param name="coordinateId">Value identifying the coordinate.</param>
        /// <param name="coordinateObject">The coordinate <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>.</param>
        /// <remarks>
        /// This method is to be called by implementations of the <see cref="IMixedRealitySpatialCoordinateObserver"/> interface, not by application code.
        /// </remarks>
        void RaiseCoordinateAdded(IMixedRealitySpatialCoordinateObserver observer, int coordinateId, SpatialCoordinate coordinateObject);

        /// <summary>
        /// <see cref="IMixedRealityNativeAnchorObserver"/>'s should call this method to indicate an existing coordinate has been updated.
        /// </summary>
        /// <param name="observer">The observer raising the event.</param>
        /// <param name="coordinateId">Value identifying the coordinate.</param>
        /// <param name="coordinateObject">The coordinate <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>.</param>
        /// <remarks>
        /// This method is to be called by implementations of the <see cref="IMixedRealitySpatialCoordinateObserver"/> interface, not by application code.
        /// </remarks>
        void RaiseCoordinateUpdated(IMixedRealitySpatialCoordinateObserver observer, int coordinateId, SpatialCoordinate coordinateObject);

        /// <summary>
        /// <see cref="IMixedRealityNativeAnchorObserver"/>'s should call this method to indicate an existing coordinate has been removed.
        /// </summary>
        /// <param name="observer">The observer raising the event.</param>
        /// <param name="coordinateId">Value identifying the coordinate.</param>
        /// <remarks>
        /// This method is to be called by implementations of the <see cref="IMixedRealitySpatialCoordinateObserver"/> interface, not by application code.
        /// </remarks>
        void RaiseCoordinateRemoved(IMixedRealitySpatialCoordinateObserver observer, int coordinateId);
    }
}
