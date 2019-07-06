// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
	/// <summary>
	/// The base interface for a spatial coordinate.
	/// </summary>	
    public interface IMixedRealitySpatialCoordinate
    {
        /// <summary>
        /// Gets the ID of the coordinate.
        /// </summary>
        string Id { get; }
    }
}