﻿//
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

namespace Microsoft.SpatialAlignment.Geocentric
{
    /// <summary>
    /// The interface for a class that provides continuous translation between
    /// <see cref="https://en.wikipedia.org/wiki/World_Geodetic_System">Geodetic</see>,
    /// <see href="https://en.wikipedia.org/wiki/ECEF">ECEF</see>, and
    /// <see cref="https://docs.unity3d.com/ScriptReference/Transform-position.html">Local</see>
    /// coordinate systems. Classes that implement this system can serve as a
    /// "point of reference" when mapping between the applications temporary
    /// local 3D space and true global 3D space.
    /// </summary>
    public interface IGeoReference
    {
        #region Public Properties
        /// <summary>
        /// Gets the <see cref="Heading"/> currently provided by the reference.
        /// </summary>
        /// <remarks>
        /// <b>IMPORTANT:</b> If the reference source is a device, this
        /// property may return <see langword = "null" /> until the device has
        /// been initialized and starts tracking.
        /// </remarks>
        HeadingData Heading { get; }

        /// <summary>
        /// Gets the <see cref="Location"/> currently provided by the reference.
        /// </summary>
        /// <remarks>
        /// <b>IMPORTANT:</b> If the reference source is a device, this
        /// property may return <see langword = "null" /> until the device has
        /// been initialized and starts tracking.
        /// </remarks>
        LocationData Location { get; }

        /// <summary>
        /// Gets a value that indicates if the source is currently tracking
        /// and providing updates.
        /// </summary>
        bool IsTracking { get; }
        #endregion // Public Properties

        #region Public Events
        /// <summary>
        /// Raised whenever the value of the <see cref="Heading"/> property
        /// has changed.
        /// </summary>
        event EventHandler HeadingChanged;

        /// <summary>
        /// Raised whenever the value of the <see cref="Location"/> property
        /// has changed.
        /// </summary>
        event EventHandler LocationChanged;
        #endregion // Public Events
    }
}