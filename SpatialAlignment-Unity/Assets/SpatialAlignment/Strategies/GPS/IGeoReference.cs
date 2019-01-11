using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// Provides a continuous "point in space" translation
    /// between a known GPS coordinate and a corresponding
    /// location in Unity's coordinate system.
    /// </summary>
    /// <remarks>
    /// <see href="https://en.wikipedia.org/wiki/Map_projection">Map Projection</see>.
    /// <see href="http://www.movable-type.co.uk/scripts/latlong.html">Map Calculations</see>.
    /// <see href="https://en.wikipedia.org/wiki/ECEF">Earth-Centered Earth-Fixed</see> (X,Y,Z).
    /// </remarks>
    public interface IGeoReference
    {
        /// <summary>
        /// Gets a <see cref="LocationInfo"/> that represents
        /// the source GPS location.
        /// </summary>
        LocationInfo GeoLocation { get; }

        /// <summary>
        /// Gets a <see cref="Vector3"/> in Unity space that
        /// represents <see cref="GeoLocation"/>.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Raised whenever the reference translation is updated.
        /// </summary>
        event EventHandler ReferenceUpdated;
    }
}