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

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    public class NativeAnchorCoordinate : BaseSpatialCoordinate
    {
        // TODO: Expose NativeAnchor
        public Object Anchor { get; protected set;  }

        /// <summary>
        /// constructor
        /// </summary>
        public NativeAnchorCoordinate()
        {
            //empty for now
        }

        /// <summary>
        /// Creates a <see cref="NativeAnchorCoordinate"/>.
        /// </summary>
        /// <param name="anchor">
        /// The native anchor the coordinate represents.
        /// <returns>
        /// A <see cref="NativeAnchorCoordinate"/> containing the anchor.
        /// </returns>
        public static NativeAnchorCoordinate Create(Object anchor)
        {
            // Create
            NativeAnchorCoordinate coordinate = new NativeAnchorCoordinate();

            // Store anchor
            coordinate.Anchor = anchor;

            // TODO: Enable
            // Store the ID
            // coordinate.Id = anchor.Id;

            // Return the created coordinate
            return coordinate;
        }
    }
}