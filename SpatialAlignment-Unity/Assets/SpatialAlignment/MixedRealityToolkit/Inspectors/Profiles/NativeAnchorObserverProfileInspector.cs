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

using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEditor;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialAlignment;
using System.Linq;

namespace Microsoft.MixedReality.Toolkit.Editor.SpatialAlignment
{
    [CustomEditor(typeof(NativeAnchorObserverProfile))]
    public class NativeAnchorObserverProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
    {
        // General settings
        private SerializedProperty startupBehavior;
        private SerializedProperty updateInterval;
        private SerializedProperty anchorIds;


        private const string ProfileTitle = "Native Anchor Observer Settings";
        private const string ProfileDescription = "Configuration settings for how native anchors will be tracked.";

        protected override void OnEnable()
        {
            base.OnEnable();

            // General settings
            startupBehavior = serializedObject.FindProperty("startupBehavior");
            updateInterval = serializedObject.FindProperty("updateInterval");
            anchorIds = serializedObject.FindProperty("anchorIds");
        }

        public override void OnInspectorGUI()
        {
            RenderProfileHeader(ProfileTitle, ProfileDescription, target, true, BackProfileType.Configuration);

            using (new GUIEnabledWrapper(!IsProfileLock((BaseMixedRealityProfile)target)))
            {
                serializedObject.Update();

                EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
                {
                    EditorGUILayout.PropertyField(startupBehavior);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(updateInterval);
                    EditorGUILayout.Space();
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Anchor Settings", EditorStyles.boldLabel);
                {
                    EditorGUILayout.PropertyField(anchorIds);
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        protected override bool IsProfileInActiveInstance()
        {
            var profile = target as BaseMixedRealityProfile;

            return MixedRealityToolkit.IsInitialized && profile != null &&
                   MixedRealityToolkit.Instance.HasActiveProfile &&
                   MixedRealityToolkit.Instance.ActiveProfile.SpatialAwarenessSystemProfile != null &&
                   MixedRealityToolkit.Instance.ActiveProfile.SpatialAwarenessSystemProfile.ObserverConfigurations != null &&
                   MixedRealityToolkit.Instance.ActiveProfile.SpatialAwarenessSystemProfile.ObserverConfigurations.Any(s => s.ObserverProfile == profile);
        }
    }
}
