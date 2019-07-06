// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    public abstract class BaseCoordinateObserverProfile : BaseMixedRealityProfile
    {
        [SerializeField]
        [Tooltip("How should the observer behave at startup?")]
        private AutoStartBehavior startupBehavior = AutoStartBehavior.AutoStart;

        /// <summary>
        /// Indicates if the observer is to start immediately or wait for manual startup.
        /// </summary>
        public AutoStartBehavior StartupBehavior => startupBehavior;

        [SerializeField]
        [Tooltip("How often, in seconds, should the coordinate observer update?")]
        private float updateInterval = 3.5f;

        /// <summary>
        /// The frequency, in seconds, at which the coordinate observer updates.
        /// </summary>
        public float UpdateInterval => updateInterval;
    }
}