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
    /// The interface for a class that stores a portion of its configuration data
    /// in a native persistence store.
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    public interface INativePersistence
    {
        /// <summary>
        /// Loads native configuration data.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method is called to load native configuration data after other
        /// configuration data has been loaded from non-native storage.
        /// </para>
        /// <para>
        /// For example, <see cref="AzureAnchorAlignment"/> uses this method to load
        /// the CloudSpatialAnchor from the ASA service after the rest of its
        /// configuration data has been loaded.
        /// </para>
        /// </remarks>
        Task LoadNativeAsync();

        /// <summary>
        /// Saves native configuration data.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method is called to save native configuration data before any
        /// remaining data is stored in non-native storage.
        /// </para>
        /// <para>
        /// For example, <see cref="AzureAnchorAlignment"/> uses this method to save
        /// the CloudSpatialAnchor to the ASA service before the rest of its
        /// configuration data can be saved.
        /// </para>
        /// </remarks>
        Task SaveNativeAsync();
    }
}
