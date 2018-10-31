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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SpatialAlignment.Persistence
{
    /// <summary>
    /// The interface for a class that can load and save spatial alignment data.
    /// </summary>
    public interface ISpatialAlignmentStore
    {
        /// <summary>
        /// Loads the frame with the specified ID.
        /// </summary>
        /// <param name="id">
        /// The ID of the frame to load.
        /// </param>
        /// <returns>
        /// The loaded frame.
        /// </returns>
        /// <remarks>
        /// If the frame is already loaded, the existing instance is returned
        /// and no error is generated.
        /// </remarks>
        Task<SpatialFrame> LoadFrameAsync(string id);

        /// <summary>
        /// Saves the specified frame.
        /// </summary>
        /// <param name="frame">
        /// The frame to save.
        /// </param>
        /// <returns>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        Task SaveFrameAsync(SpatialFrame frame);
    }
}
