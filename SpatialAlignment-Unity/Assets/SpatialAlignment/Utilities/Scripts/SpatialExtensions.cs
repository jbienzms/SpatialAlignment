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
        #region GameObject Extensions
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
        #endregion // GameObject Extensions

        #region RefinementDirection Extensions
        /// <summary>
        /// Gets a direction relative to the specified direction.
        /// </summary>
        /// <param name="direction">
        /// The direction used to obtain the relative direction.
        /// </param>
        /// <returns>
        /// The relative direction.
        /// </returns>
        static public RefinementDirection RelativeTo(this RefinementDirection direction, RefinementDirection relative)
        {
            switch (direction)
            {
                case RefinementDirection.Forward:
                    switch (relative)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Forward;
                        case RefinementDirection.Back:
                            return RefinementDirection.Back;
                        case RefinementDirection.Down:
                            return RefinementDirection.Down;
                        case RefinementDirection.Left:
                            return RefinementDirection.Left;
                        case RefinementDirection.Right:
                            return RefinementDirection.Right;
                        case RefinementDirection.Up:
                            return RefinementDirection.Up;
                        default:
                            throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {relative}");
                    }

                case RefinementDirection.Back:
                    switch (relative)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Back;
                        case RefinementDirection.Back:
                            return RefinementDirection.Forward;
                        case RefinementDirection.Down:
                            return RefinementDirection.Down;
                        case RefinementDirection.Left:
                            return RefinementDirection.Right;
                        case RefinementDirection.Right:
                            return RefinementDirection.Left;
                        case RefinementDirection.Up:
                            return RefinementDirection.Up;
                        default:
                            throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {relative}");
                    }

                case RefinementDirection.Down:
                    switch (relative)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Down;
                        case RefinementDirection.Back:
                            return RefinementDirection.Up;
                        case RefinementDirection.Down:
                            return RefinementDirection.Back;
                        case RefinementDirection.Left:
                            return RefinementDirection.Left;
                        case RefinementDirection.Right:
                            return RefinementDirection.Right;
                        case RefinementDirection.Up:
                            return RefinementDirection.Forward;
                        default:
                            throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {relative}");
                    }

                case RefinementDirection.Left:
                    switch (relative)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Left;
                        case RefinementDirection.Back:
                            return RefinementDirection.Right;
                        case RefinementDirection.Down:
                            return RefinementDirection.Down;
                        case RefinementDirection.Left:
                            return RefinementDirection.Back;
                        case RefinementDirection.Right:
                            return RefinementDirection.Forward;
                        case RefinementDirection.Up:
                            return RefinementDirection.Up;
                        default:
                            throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {relative}");
                    }

                case RefinementDirection.Right:
                    switch (relative)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Right;
                        case RefinementDirection.Back:
                            return RefinementDirection.Left;
                        case RefinementDirection.Down:
                            return RefinementDirection.Down;
                        case RefinementDirection.Left:
                            return RefinementDirection.Forward;
                        case RefinementDirection.Right:
                            return RefinementDirection.Back;
                        case RefinementDirection.Up:
                            return RefinementDirection.Up;
                        default:
                            throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {relative}");
                    }

                case RefinementDirection.Up:
                    switch (relative)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Up;
                        case RefinementDirection.Back:
                            return RefinementDirection.Down;
                        case RefinementDirection.Down:
                            return RefinementDirection.Forward;
                        case RefinementDirection.Left:
                            return RefinementDirection.Left;
                        case RefinementDirection.Right:
                            return RefinementDirection.Right;
                        case RefinementDirection.Up:
                            return RefinementDirection.Back;
                        default:
                            throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {relative}");
                    }

                default:
                    throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {direction}");
            }
        }

        /// <summary>
        /// Converts a <see cref="RefinementDirection"/> into a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="direction">
        /// The <see cref="RefinementDirection"/> to convert.
        /// </param>
        /// <returns>
        /// The <see cref="Vector3"/> that represents the direction.
        /// </returns>
        static public Vector3 ToVector(this RefinementDirection direction)
        {
            switch (direction)
            {
                case RefinementDirection.Forward:
                    return Vector3.forward;
                case RefinementDirection.Back:
                    return Vector3.back;
                case RefinementDirection.Down:
                    return Vector3.down;
                case RefinementDirection.Left:
                    return Vector3.left;
                case RefinementDirection.Right:
                    return Vector3.right;
                case RefinementDirection.Up:
                    return Vector3.up;
                default:
                    throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {direction}");
            }
        }
        #endregion // RefinementDirection Extensions
    }
}
