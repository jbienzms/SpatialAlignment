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
    /// Defines the potential states of alignment for a strategy.
    /// </summary>
    public enum AlignmentState
    {
        /// <summary>
        /// Alignment has never been resolved in this session.
        /// </summary>
        Unresolved,

        /// <summary>
        /// Alignment is temporarily inhibited (usually due to some form of tracking loss).
        /// </summary>
        Inhibited,

        /// <summary>
        /// Alignment has been resolved once but there is no active tracking.
        /// </summary>
        Resolved,

        /// <summary>
        /// Alignment has been resolved and there is active tracking.
        /// </summary>
        Tracking,
    }

    /// <summary>
    /// The interface for a class that can align an object spatially.
    /// </summary>
    public interface IAlignmentStrategy
    {
        #region Public Properties
        /// <summary>
        /// Gets the current accuracy of the alignment (in meters).
        /// </summary>
        /// <remarks>
        /// If the <see cref="State"/> of the strategy is
        /// <see cref="AlignmentState.Unresolved">Unresolved</see> or
        /// <see cref="AlignmentState.Inhibited">Inhibited</see>,
        /// this property may return <see cref="Vector3.positiveInfinity"/>
        /// to indicate that the accuracy is unknown.
        /// </remarks>
        Vector3 Accuracy { get; }

        /// <summary>
        /// Gets or sets a unique ID for this instance of the strategy.
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// Gets the current state of the alignment.
        /// </summary>
        AlignmentState State { get; }
        #endregion // Public Properties

        #region Public Events
        /// <summary>
        /// Occurs when the value of the <see cref="Accuracy"/> property has changed.
        /// </summary>
        event EventHandler AccuracyChanged;

        /// <summary>
        /// Occurs when the value of the <see cref="State"/> property has changed.
        /// </summary>
        event EventHandler StateChanged;
        #endregion // Public Events
    }
}
