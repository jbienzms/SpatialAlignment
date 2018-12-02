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

using HoloToolkit.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// Extensions methods for Unity classes.
    /// </summary>
    static public class SpatialExtensions
    {
        /// <summary>
        /// Perform an action on every component of type T that is on this
        /// GameObject and its children
        /// </summary>
        /// <typeparam name="T">Component Type</typeparam>
        /// <param name="g">this gameObject</param>
        /// <param name="action">Action to perform.</param>
        static public void ForEachComponentInChildren<T>(this GameObject g, Action<T> action)
        {
            foreach (T i in g.GetComponentsInChildren<T>())
            {
                action(i);
            }
        }

        /// <summary>
        /// Enables or disables all meshes on the specified GameObject.
        /// </summary>
        /// <param name="enabled">
        /// Whether meshes are enabled.
        /// </param>
        /// <param name="inChildren">
        /// Whether to modify meshes in child GameObjects as well. The default
        /// is <c>false</c>.
        /// </param>
        /// <param name="setColliders">
        /// Whether colliders should match. The default is <c>true</c>.
        /// </param>
        static public void SetMeshesEnabled(this GameObject g, bool enabled, bool inChildren = false, bool setColliders = true)
        {
            if (inChildren)
            {
                g.ForEachComponentInChildren<MeshRenderer>(r => r.enabled = enabled);
            }
            else
            {
                g.ForEachComponent<MeshRenderer>(r => r.enabled = enabled);
            }

            if (setColliders)
            {
                if (inChildren)
                {
                    g.ForEachComponentInChildren<Collider>(c => c.enabled = enabled);
                }
                else
                {
                    g.ForEachComponent<Collider>(c => c.enabled = enabled);
                }
            }
        }
    }
}
