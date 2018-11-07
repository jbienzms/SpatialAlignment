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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// A lookup collection of <see cref="SpatialFrame"/>s, keyed by their
    /// <see cref="SpatialFrame.Id">Id</see>s.
    /// </summary>
    public class SpatialFrameCollection : KeyedCollection<string, SpatialFrame>
    {
        #region Overridables / Event Triggers
        /// <summary>
        /// Called when the ID of the frame has changed.
        /// </summary>
        /// <param name="sender">
        /// The frame.
        /// </param>
        /// <param name="e">
        /// An <see cref="EventArgs"/> that contains event data.
        /// </param>
        /// <remarks>
        /// The base implementation of this method updates the key lookup
        /// table to use the new ID.
        /// </remarks>
        protected virtual void OnIdChanged(object sender, EventArgs e)
        {
            SpatialFrame frame = (SpatialFrame)sender;
            ChangeItemKey(frame, frame.Id);
        }
        #endregion // Overridables / Event Triggers

        #region Overrides / Event Handlers
        /// <inheritdoc />
        protected override void ClearItems()
        {
            // Unsubscribe from all ID change notifications
            foreach (SpatialFrame frame in Items)
            {
                frame.IdChanged -= OnIdChanged;
            }

            // Clear the list
            base.ClearItems();
        }

        /// <inheritdoc />
        protected override string GetKeyForItem(SpatialFrame item)
        {
            return item.Id;
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, SpatialFrame item)
        {
            // Let base add to collection first
            base.InsertItem(index, item);

            // Subscribe to ID change notifications
            item.IdChanged += OnIdChanged;
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            // Get the removed item
            SpatialFrame removedItem = Items[index];

            // Unsubscribe from ID change notifications
            removedItem.IdChanged -= OnIdChanged;

            // Let base remove
            base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void SetItem(int index, SpatialFrame item)
        {
            // Get the replaced item
            SpatialFrame replacedItem = Items[index];

            // Unsubscribe from ID change notifications
            replacedItem.IdChanged -= OnIdChanged;

            // Let base replace
            base.SetItem(index, item);
        }
        #endregion // Overrides / Event Handlers

        #region Public Methods
        /// <summary>
        /// Adds all of the items from the specified collection.
        /// </summary>
        /// <param name="collection">
        /// The collection which contains the items to add.
        /// </param>
        public virtual void AddRange(IEnumerable<SpatialFrame> collection)
        {
            // Validate
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            // Add
            foreach (SpatialFrame frame in collection)
            {
                this.Add(frame);
            }
        }
        #endregion // Public Methods
    }
}
