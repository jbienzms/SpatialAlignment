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
using UnityEngine;

#if !NO_MRTK
using HoloToolkit.Unity;
#endif

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
            switch (relative)
            {
                case RefinementDirection.Forward: // Facing Forward
                    switch (direction)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Forward; // Facing Forward Looking Forward is Forward
                        case RefinementDirection.Back:
                            return RefinementDirection.Back;    // Facing Forward Looking Back is Back
                        case RefinementDirection.Down:
                            return RefinementDirection.Down;    // Facing Forward Looking Down is Down
                        case RefinementDirection.Left:
                            return RefinementDirection.Left;    // Facing Forward Looking Left is Left
                        case RefinementDirection.Right:
                            return RefinementDirection.Right;   // Facing Forward Looking Right is Right
                        case RefinementDirection.Up:
                            return RefinementDirection.Up;      // Facing Forward Looking Up is Up
                        default:
                            throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {relative}");
                    }

                case RefinementDirection.Back: // Back Looking
                    switch (direction)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Back;    // Facing Back Looking Forward is Back
                        case RefinementDirection.Back:
                            return RefinementDirection.Forward; // Facing Back Looking Back is Forward
                        case RefinementDirection.Down:
                            return RefinementDirection.Down;    // Facing Back Looking Down is Down
                        case RefinementDirection.Left:
                            return RefinementDirection.Right;   // Facing Back Looking Left is Right
                        case RefinementDirection.Right:
                            return RefinementDirection.Left;    // Facing Back Looking Right is Left
                        case RefinementDirection.Up:
                            return RefinementDirection.Up;      // Facing Back Looking Up is Up
                        default:
                            throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {relative}");
                    }

                case RefinementDirection.Down: // Facing Down
                    switch (direction)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Down;    // Facing Down Looking Forward is Down
                        case RefinementDirection.Back:
                            return RefinementDirection.Up;      // Facing Back Looking Back is Up
                        case RefinementDirection.Down:
                            return RefinementDirection.Back;    // Facing Down Looking Down is Back
                        case RefinementDirection.Left:
                            return RefinementDirection.Left;    // Facing Down Looking Left is Left
                        case RefinementDirection.Right:
                            return RefinementDirection.Right;   // Facing Down Looking Right is Right
                        case RefinementDirection.Up:
                            return RefinementDirection.Forward; // Facing Down Looking Up is Forward
                        default:
                            throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {relative}");
                    }

                case RefinementDirection.Left: // Facing Left
                    switch (direction)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Left;    // Facing Left Looking Forward is Left
                        case RefinementDirection.Back:
                            return RefinementDirection.Right;   // Facing Left Looking Back is Right
                        case RefinementDirection.Down:
                            return RefinementDirection.Down;    // Facing Left Looking Down is Down
                        case RefinementDirection.Left:
                            return RefinementDirection.Back;    // Facing Left Looking Left is Back
                        case RefinementDirection.Right:
                            return RefinementDirection.Forward; // Facing Left Looking Right is Forward
                        case RefinementDirection.Up:
                            return RefinementDirection.Up;      // Facing Left Looking Up is Up
                        default:
                            throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {relative}");
                    }

                case RefinementDirection.Right: // Facing Right
                    switch (direction)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Right;	// Facing Right Looking Forward is Forward
                        case RefinementDirection.Back:
                            return RefinementDirection.Left;    // Facing Right Looking Back is Left
                        case RefinementDirection.Down:
                            return RefinementDirection.Down;    // Facing Right Looking Down is Down
                        case RefinementDirection.Left:
                            return RefinementDirection.Forward; // Facing Right Looking Left is Forward
                        case RefinementDirection.Right:
                            return RefinementDirection.Back;    // Facing Right Looking Right is Back
                        case RefinementDirection.Up:
                            return RefinementDirection.Up;      // Facing Right Looking Up is Up
                        default:
                            throw new InvalidOperationException($"Unknown {nameof(RefinementDirection)}: {relative}");
                    }

                case RefinementDirection.Up: // Facing Up
                    switch (direction)
                    {
                        case RefinementDirection.Forward:
                            return RefinementDirection.Up;		// Facing Up Looking Forward is Up
                        case RefinementDirection.Back:
                            return RefinementDirection.Down;	// Facing Up Looking Back is Down
                        case RefinementDirection.Down:
                            return RefinementDirection.Forward; // Facing Up Looking Down is Forward
                        case RefinementDirection.Left:
                            return RefinementDirection.Left;    // Facing Up Looking Left is Left
                        case RefinementDirection.Right:
                            return RefinementDirection.Right;   // Facing Up Looking Right is Right
                        case RefinementDirection.Up:
                            return RefinementDirection.Back;    // Facing Up Looking Up is Back
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

        /// <summary>
        /// Attempts to convert a <see cref="Vector3"/> into a <see cref="RefinementDirection"/>.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector3"/> to convert.
        /// </param>
        /// <param name="direction">
        /// The output <see cref="RefinementDirection"/> if successfully converted.
        /// </param>
        /// <returns>
        /// <c>true</c> if the conversion was successful; otherwise <c>false</c>.
        /// </returns>
        static public bool TryGetDirection(this Vector3 vector, out RefinementDirection direction)
        {
            // Default to forward
            direction = RefinementDirection.Forward;

            // What is the vector?
            if (vector == Vector3.back)
            {
                direction = RefinementDirection.Back;
                return true;
            }
            else if(vector == Vector3.down)
            {
                direction = RefinementDirection.Down;
                return true;
            }
            else if (vector == Vector3.forward)
            {
                direction = RefinementDirection.Forward;
                return true;
            }
            else if (vector == Vector3.left)
            {
                direction = RefinementDirection.Left;
                return true;
            }
            else if (vector == Vector3.right)
            {
                direction = RefinementDirection.Right;
                return true;
            }
            else if (vector == Vector3.up)
            {
                direction = RefinementDirection.Up;
                return true;
            }

            // No conversion possible
            return false;
        }
        #endregion // RefinementDirection Extensions

        #region Transform Extensions
        /// <summary>
        /// Updates the transform to match the specified world values.
        /// </summary>
        /// <param name="transform">
        /// The transform to update.
        /// </param>
        /// <param name="position">
        /// The updated position.
        /// </param>
        /// <param name="rotation">
        /// The updated rotation.
        /// </param>
        /// <param name="scale">
        /// The updated scale.
        /// </param>
        /// <remarks>
        /// Animations only work in MRTK builds. In other builds animations complete immediately.
        /// </remarks>
        static public void AnimateTo(this Transform transform, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            #if NO_MRTK
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
            #else
            // Get or create Interpolator
            Interpolator i = transform.gameObject.EnsureComponent<Interpolator>();

            // Reset in case of previous action
            i.Reset();

            // Interpolate to values
            i.SetTargetPosition(position);
            i.SetTargetRotation(rotation);
            i.SetTargetLocalScale(scale);
            #endif
        }

        /// <summary>
        /// Skips any running animation to the end.
        /// </summary>
        /// <param name="transform">
        /// The transform where an animation may be running.
        /// </param>
        /// <remarks>
        /// Animations only work in MRTK builds. In other builds animations complete immediately.
        /// </remarks>
        static public void EndAnimation(this Transform transform)
        {
            #if !NO_MRTK
            // Try to get Interpolator
            Interpolator i = transform.gameObject.GetComponent<Interpolator>();

            // If found, jump to end
            if (i != null) { i.SnapToTarget(); }
            #endif
        }
        #endregion // Transform Extensions

        #region Vector Extensions
        /// <summary>
        /// Returns a new <see cref="Vector3"/> that contains only the greatest
        /// absolute value on any axis.
        /// </summary>
        /// <param name="vector">
        /// The vector to obtain the absolute axis for.
        /// </param>
        /// <returns>
        /// A <see cref="Vector3"/> with the greatest absolute axis.
        /// If any two axis are exactly equal, this method returns
        /// <see cref="Vector3.zero"/>.
        /// </returns>
        static public Vector3 AbsoluteAxis(this Vector3 vector)
        {
            // Placeholder
            Vector3 result = Vector3.zero;

            // Get the absolute value of the incoming vector
            Vector3 abs = new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));

            // Select larges axis
            if ((abs.x > abs.y) && (abs.x > abs.z))
            {
                result.x = vector.x;
            }
            else if ((abs.y > abs.x) && (abs.y > abs.z))
            {
                result.y = vector.y;
            }
            else if ((abs.z > abs.x) && (abs.z > abs.y))
            {
                result.z = vector.z;
            }

            // Done
            return result;
        }

        /// <summary>
        /// Rounds a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="vector">
        /// The vector to round
        /// </param>
        /// <param name="decimalPlaces">
        /// The number of decimal places to round to. The default is 0.
        /// </param>
        /// <returns>
        /// The rounded vector.
        /// </returns>
        static public Vector3 Round(this Vector3 vector, int decimalPlaces = 0)
        {
            float multiplier = 1;
            if (decimalPlaces > 0)
            {
                multiplier = Mathf.Pow(10f, decimalPlaces);
            }
            return new Vector3(
                       Mathf.Round(vector.x * multiplier) / multiplier,
                       Mathf.Round(vector.y * multiplier) / multiplier,
                       Mathf.Round(vector.z * multiplier) / multiplier);
        }

        /// <summary>
        /// Returns a weighted percentage of the specified vector.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector3"/> to calculate the weighted value for.
        /// </param>
        /// <param name="weight">
        /// The weighted percentage to apply.
        /// </param>
        /// <returns>
        /// The weighted <see cref="Vector3"/>.
        /// </returns>
        static public Vector3 Weighted(this Vector3 vector, float weight)
        {
            return new Vector3(vector.x * weight, vector.y * weight, vector.z * weight);
        }

        /// <summary>
        /// Returns a weighted percentage of the specified vector.
        /// </summary>
        /// <param name="vector">
        /// The <see cref="Vector3"/> to calculate the weighted value for.
        /// </param>
        /// <param name="weight">
        /// The weighted percentage to apply.
        /// </param>
        /// <returns>
        /// The weighted <see cref="Vector3"/>.
        /// </returns>
        static public Quaternion Weighted(this Quaternion quat, float weight)
        {
            return Quaternion.Lerp(Quaternion.identity, quat, weight);
        }
        #endregion // Vector Extensions
    }
}
