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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// Provides a continuous "point of reference" translation
    /// between a known GPS coordinate and its corresponding
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