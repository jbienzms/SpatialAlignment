// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    public interface IMixedRealitySpatialCoordinateHandler<T> : IEventSystemHandler  where T : ISpatialCoordinate
    {
        /// <summary>
        /// Called when a coordinate observer adds a new coordinate.
        /// </summary>
        /// <param name="eventData">Data describing the event.</param>
        void OnCoordinateAdded(MixedRealityCoordinateEventData<T> eventData);

        /// <summary>
        /// Called when a coordinate observer updates an existing coordinate.
        /// </summary>
        /// <param name="eventData">Data describing the event.</param>
        void OnCoordinateUpdated(MixedRealityCoordinateEventData<T> eventData);

        /// <summary>
        /// Called when a coordinate observer removes an existing coordinate.
        /// </summary>
        /// <param name="eventData">Data describing the event.</param>
        void OnCoordinateRemoved(MixedRealityCoordinateEventData<T> eventData);
    }
}
