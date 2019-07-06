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

using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    /// <summary>
    /// Data for spatial coordinate events.
    /// </summary>
    /// <typeparam name="T">The spatial coordinate data type.</typeparam>
    public class SpatialCoordinateEventData<T> : GenericBaseEventData where T : ISpatialCoordinate
    {
        /// <inheritdoc />
        public SpatialCoordinateEventData(EventSystem eventSystem) : base(eventSystem) { }

        /// <summary>
        /// Initialize the event data.
        /// </summary>
        /// <param name="observer">
        /// The <see cref="ISpatialCoordinateObserver"/> that raised the event.
        /// </param>
        /// <param name="coordinateId">
        /// The ID of the coordinate.
        /// </param>
        /// <param name="coordinate">
        /// The coordinate instance, if available.
        /// </param>
        public void Initialize(ISpatialCoordinateObserver observer, string coordinateId, T coordinate)
        {
            BaseInitialize(observer);
            CoordinateId = coordinateId;
            Coordinate = coordinate;
        }

        /// <summary>
        /// Gets the spatial coordinate to which this event pertains.
        /// </summary>
        /// <remarks>
        /// The coordinate is not available when the coordinate has been
        /// removed. Instead, see the value of <see cref="CoordinateId"/>.
        /// </remarks>
        public T Coordinate { get; protected set; }

        /// <summary>
        /// Gets the ID of the spatial coordinate.
        /// </summary>
        /// <remarks>
        /// This value is always available, even when the coordinate has been
        /// removed.
        /// </remarks>
        public string CoordinateId { get; protected set; }
    }

    /// <summary>
    /// Data for untyped spatial coordinate events.
    /// </summary>
    public class SpatialCoordinateEventData : SpatialCoordinateEventData<ISpatialCoordinate>
    {
        /// <inheritdoc />
        public SpatialCoordinateEventData(EventSystem eventSystem) : base(eventSystem) { }
    }
}
