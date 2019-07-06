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

using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using UnityEngine;
using UnityEditor;
using Microsoft.MixedReality.Toolkit.Utilities;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment.Editor
{
    [CustomEditor(typeof(SpatialAlignmentSystemProfile))]
    public class SpatialAlignmentSystemProfileInspector : BaseMixedRealityToolkitConfigurationProfileInspector
    {
        private static readonly GUIContent AddObserverContent = new GUIContent("+ Add Coordinate Observer", "Add Coordinate Observer");
        private static readonly GUIContent RemoveObserverContent = new GUIContent("-", "Remove Coordinate Observer");

        private static readonly GUIContent ComponentTypeContent = new GUIContent("Type");
        private static readonly GUIContent RuntimePlatformContent = new GUIContent("Platform(s)");

        private SerializedProperty observerConfigurations;

        private const string ProfileTitle = "Spatial Alignment System Settings";
        private const string ProfileDescription = "The Spatial Alignment System profile allows developers to configure cross-platform coordinate tracking.";

        private static bool[] observerFoldouts;

        protected override void OnEnable()
        {
            base.OnEnable();

            observerConfigurations = serializedObject.FindProperty("observerConfigurations");

            if (observerFoldouts == null || observerFoldouts.Length != observerConfigurations.arraySize)
            {
                observerFoldouts = new bool[observerConfigurations.arraySize];
            }
        }

        public override void OnInspectorGUI()
        {
            RenderProfileHeader(ProfileTitle, ProfileDescription, target);

            using (new GUIEnabledWrapper(!IsProfileLock((BaseMixedRealityProfile)target)))
            {
                serializedObject.Update();

                using (new EditorGUI.IndentLevelScope())
                {
                    RenderList(observerConfigurations);
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        protected override bool IsProfileInActiveInstance()
        {
            var profile = target as BaseMixedRealityProfile;
            return MixedRealityToolkit.IsInitialized && profile != null &&
                   MixedRealityToolkit.Instance.HasActiveProfile &&
                   profile == MixedRealityToolkit.Instance.ActiveProfile;
        }

        private void RenderList(SerializedProperty list)
        {
            bool changed = false;

            using (new EditorGUILayout.VerticalScope())
            {
                if (InspectorUIUtility.RenderIndentedButton(AddObserverContent, EditorStyles.miniButton))
                {
                    list.InsertArrayElementAtIndex(list.arraySize);
                    SerializedProperty observer = list.GetArrayElementAtIndex(list.arraySize - 1);

                    SerializedProperty observerName = observer.FindPropertyRelative("componentName");
                    observerName.stringValue = $"New spatial observer {list.arraySize - 1}";

                    SerializedProperty runtimePlatform = observer.FindPropertyRelative("runtimePlatform");
                    runtimePlatform.intValue = -1;

                    SerializedProperty configurationProfile = observer.FindPropertyRelative("observerProfile");
                    configurationProfile.objectReferenceValue = null;

                    serializedObject.ApplyModifiedProperties();

                    SystemType observerType = ((SpatialAlignmentSystemProfile)serializedObject.targetObject).ObserverConfigurations[list.arraySize - 1].ComponentType;
                    observerType.Type = null;

                    observerFoldouts = new bool[list.arraySize];
                    return;
                }

                if (list == null || list.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("The Mixed Reality Spatial Alignment System requires one or more observers.", MessageType.Warning);
                    return;
                }

                for (int i = 0; i < list.arraySize; i++)
                {
                    SerializedProperty observer = list.GetArrayElementAtIndex(i);
                    SerializedProperty observerName = observer.FindPropertyRelative("componentName");
                    SerializedProperty observerType = observer.FindPropertyRelative("componentType");
                    SerializedProperty observerProfile = observer.FindPropertyRelative("observerProfile");
                    SerializedProperty runtimePlatform = observer.FindPropertyRelative("runtimePlatform");

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            observerFoldouts[i] = EditorGUILayout.Foldout(observerFoldouts[i], observerName.stringValue, true);

                            if (GUILayout.Button(RemoveObserverContent, EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                            {
                                list.DeleteArrayElementAtIndex(i);
                                serializedObject.ApplyModifiedProperties();
                                changed = true;
                                break;
                            }
                        }

                        if (observerFoldouts[i])
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(observerType, ComponentTypeContent);
                            if (EditorGUI.EndChangeCheck())
                            {
                                serializedObject.ApplyModifiedProperties();
                                System.Type type = ((SpatialAlignmentSystemProfile)serializedObject.targetObject).ObserverConfigurations[i].ComponentType.Type;
                                ApplyObserverConfiguration(type, observerName, observerProfile, runtimePlatform);
                                break;
                            }

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(runtimePlatform, RuntimePlatformContent);
                            changed |= EditorGUI.EndChangeCheck();

                            System.Type serviceType = null;
                            if (observerProfile.objectReferenceValue != null)
                            {
                                serviceType = (target as SpatialAlignmentSystemProfile).ObserverConfigurations[i].ComponentType;
                            }

                            changed |= RenderProfile(observerProfile, null, true, false, serviceType);

                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }

                if (changed && MixedRealityToolkit.IsInitialized)
                {
                    EditorApplication.delayCall += () => MixedRealityToolkit.Instance.ResetConfiguration(MixedRealityToolkit.Instance.ActiveProfile);
                }
            }
        }
        private void ApplyObserverConfiguration(
            System.Type type,
            SerializedProperty observerName,
            SerializedProperty configurationProfile,
            SerializedProperty runtimePlatform)
        {
            if (type != null)
            {
                MixedRealityDataProviderAttribute observerAttribute = MixedRealityDataProviderAttribute.Find(type) as MixedRealityDataProviderAttribute;
                if (observerAttribute != null)
                {
                    observerName.stringValue = !string.IsNullOrWhiteSpace(observerAttribute.Name) ? observerAttribute.Name : type.Name;
                    configurationProfile.objectReferenceValue = observerAttribute.DefaultProfile;
                    runtimePlatform.intValue = (int)observerAttribute.RuntimePlatforms;
                }
                else
                {
                    observerName.stringValue = type.Name;
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}