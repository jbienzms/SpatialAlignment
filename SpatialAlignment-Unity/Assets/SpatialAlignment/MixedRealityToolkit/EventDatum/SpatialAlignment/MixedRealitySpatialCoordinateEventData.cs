// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    /// <summary>
    /// Data for spatial coordinate events.
    /// </summary>
    public class MixedRealitySpatialCoordinateEventData : GenericBaseEventData
    {
        /// <summary>
        /// Identifier of the coordinate associated with this event.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventSystem"></param>
        public MixedRealitySpatialCoordinateEventData(EventSystem eventSystem) : base(eventSystem) { }

        /// <summary>
        /// Initialize the event data.
        /// </summary>
        /// <param name="observer">The <see cref="IMixedRealitySpatialCoordinateObserver"/> that raised the event.</param>
        /// <param name="id">The identifier of the observed spatial coordinate.</param>
        public void Initialize(IMixedRealitySpatialCoordinateObserver observer, int id)
        {
            BaseInitialize(observer);
            Id = id;
        }
    }

    /// <summary>
    /// Data for spatial coordinate events.
    /// </summary>
    /// <typeparam name="T">The spatial coordinate data type.</typeparam>
    public class MixedRealitySpatialCoordinateEventData<T> : MixedRealitySpatialCoordinateEventData where T : ISpatialCoordinate
    {
        /// <summary>
        /// The spatial coordinate to which this event pertains.
        /// </summary>
        public T SpatialCoordinate { get; private set; }

        /// <inheritdoc />
        public MixedRealitySpatialCoordinateEventData(EventSystem eventSystem) : base(eventSystem) { }

        /// <summary>
        /// Initialize the event data.
        /// </summary>
        /// <param name="observer">The <see cref="IMixedRealitySpatialCoordinateObserver"/> that raised the event.</param>
        /// <param name="id">The identifier of the observed spatial coordinate.</param>
        /// <param name="spatialCoordinate">The observed spatial coordinate.</param>
        public void Initialize(IMixedRealitySpatialCoordinateObserver observer, int id, T spatialCoordinate)
        {
            Initialize(observer, id);
            SpatialCoordinate = spatialCoordinate;
        }
    }
}
