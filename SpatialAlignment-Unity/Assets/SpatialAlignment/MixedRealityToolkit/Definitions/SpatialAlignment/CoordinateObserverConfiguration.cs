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

using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    [Serializable]
    public struct CoordinateObserverConfiguration : IMixedRealityServiceConfiguration
    {
        [SerializeField]
        [Implements(typeof(ISpatialCoordinateObserver), TypeGrouping.ByNamespaceFlat)]
        private SystemType componentType;

        /// <inheritdoc />
        public SystemType ComponentType => componentType;

        [SerializeField]
        private string componentName;

        /// <inheritdoc />
        public string ComponentName => componentName;

        [SerializeField]
        private uint priority;

        /// <inheritdoc />
        public uint Priority => priority;

        [SerializeField]
        [EnumFlags]
        private SupportedPlatforms runtimePlatform;

        /// <inheritdoc />
        public SupportedPlatforms RuntimePlatform => runtimePlatform;

        [SerializeField]
        private BaseCoordinateObserverProfile observerProfile;

        /// <summary>
        ///
        /// </summary>
        public BaseCoordinateObserverProfile ObserverProfile => observerProfile;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="componentType">The <see cref="Microsoft.MixedReality.Toolkit.Utilities.SystemType"/> of the observer.</param>
        /// <param name="componentName">The friendly name of the observer.</param>
        /// <param name="priority">The load priority of the observer.</param>
        /// <param name="runtimePlatform">The runtime platform(s) supported by the observer.</param>
        /// <param name="configurationProfile">The configuration profile for the observer.</param>
        public CoordinateObserverConfiguration(
            SystemType componentType,
            string componentName,
            uint priority,
            SupportedPlatforms runtimePlatform,
            BaseCoordinateObserverProfile configurationProfile)
        {
            this.componentType = componentType;
            this.componentName = componentName;
            this.priority = priority;
            this.runtimePlatform = runtimePlatform;
            this.observerProfile = configurationProfile;
        }
    }
}
