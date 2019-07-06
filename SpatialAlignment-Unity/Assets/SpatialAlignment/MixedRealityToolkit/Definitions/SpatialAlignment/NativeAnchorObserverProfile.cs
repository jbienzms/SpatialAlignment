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

using Microsoft.MixedReality.Toolkit.Physics;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    /// <summary>
    /// Configuration profile settings for spatial awareness mesh observers.
    /// </summary>
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Profiles/Alignment/Native Anchor Observer", fileName = "NativeAnchorObserverProfile")]
    [MixedRealityServiceProfile(typeof(ISpatialCoordinateObserver))]
    [DocLink("https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/SpatialAlignment/SpatialAlignmentGettingStarted.html")]
    public class NativeAnchorObserverProfile : BaseCoordinateObserverProfile
    {
        #region Native Anchor Settings

        [Tooltip("Physics layer on which to set observed meshes.")]
        [SerializeField]
        private string[] anchorIds = new string[0];

        public string[] AnchorIds
        {
            get { return anchorIds; }
            internal set { anchorIds = value; }
        }
        #endregion // Native Anchor Settings
    }
}
