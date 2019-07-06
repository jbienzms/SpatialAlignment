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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    /// <summary>
    /// Class providing the default implementation of the <see cref="IMixedRealitySpatialAlignmentSystem"/> interface.
    /// </summary>
    [DocLink("https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/SpatialAlignment/SpatialAlignmentGettingStarted.html")]
    public class MixedRealitySpatialAlignmentSystem : BaseCoreSystem, ISpatialAlignmentSystem, IMixedRealityDataProviderAccess, IMixedRealityCapabilityCheck, IMixedRealityExtensionService
    {
        public MixedRealitySpatialAlignmentSystem(
            IMixedRealityServiceRegistrar registrar,
            SpatialAlignmentSystemProfile profile) : base(registrar, profile)
        {
            if (registrar == null)
            {
                Debug.LogError("The MixedRealitySpatialAlignmentSystem object requires a valid IMixedRealityServiceRegistrar instance.");
            }
        }

        #region IMixedRealityCapabilityCheck Implementation

        /// <inheritdoc />
        public bool CheckCapability(MixedRealityCapability capability)
        {
            for (int i = 0; i < observers.Count; i++)
            {
                IMixedRealityCapabilityCheck capabilityChecker = observers[i] as IMixedRealityCapabilityCheck;

                // If one of the running data providers supports the requested capability,
                // the application has the needed support to leverage the desired functionality.
                if ((capabilityChecker != null) &&
                        capabilityChecker.CheckCapability(capability))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion IMixedRealityCapabilityCheck Implementation

        #region IMixedRealityToolkitService Implementation

        private SpatialCoordinateEventData<ISpatialCoordinate> coordinateEventData = null;

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();
            InitializeInternal();
        }

        /// <summary>
        /// Performs initialization tasks for the spatial alignment system.
        /// </summary>
        private void InitializeInternal()
        {
            coordinateEventData = new SpatialCoordinateEventData<ISpatialCoordinate>(EventSystem.current);

            #if UNITY_EDITOR
            // No need to enable capabilities at this level, but perhaps at the provider level. For example, QR Code.
            #endif // UNITY_EDITOR
        }

        /// <inheritdoc/>
        public override void Disable()
        {
            base.Disable();

            if (observers.Count > 0)
            {
                // Unregister the coordinate observers
                for (int i = 0; i < observers.Count; i++)
                {
                    if (observers[i] != null)
                    {
                        Registrar.UnregisterDataProvider<ISpatialCoordinateObserver>(observers[i]);
                    }
                }
            }
            observers.Clear();
        }

        /// <inheritdoc/>
        public override void Enable()
        {
            base.Enable();

            SpatialAlignmentSystemProfile profile = ConfigurationProfile as SpatialAlignmentSystemProfile;

            if ((observers.Count == 0) && (profile != null))
            {
                // Register the spatial observers.
                for (int i = 0; i < profile.ObserverConfigurations.Length; i++)
                {
                    CoordinateObserverConfiguration configuration = profile.ObserverConfigurations[i];
                    object[] args = { Registrar, this, configuration.ComponentName, configuration.Priority, configuration.ObserverProfile };

                    if (Registrar.RegisterDataProvider<ISpatialCoordinateObserver>(
                                configuration.ComponentType.Type,
                                configuration.RuntimePlatform,
                                args))
                    {
                        observers.Add(Registrar.GetDataProvider<ISpatialCoordinateObserver>(configuration.ComponentName));
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            Disable();
            Initialize();
            Enable();
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            #if UNITY_EDITOR
            // No need to disable capabilities at this level, but perhaps at the provider level. For example, QR Code.
            #endif // UNITY_EDITOR

            // Cleanup game objects created during execution.
            if (Application.isPlaying)
            {
                // Detach the child objects and clean up the parent.
                if (spatialAlignmentObjectParent != null)
                {
                    if (Application.isEditor)
                    {
                        Object.DestroyImmediate(spatialAlignmentObjectParent);
                    }
                    else
                    {
                        spatialAlignmentObjectParent.transform.DetachChildren();
                        Object.Destroy(spatialAlignmentObjectParent);
                    }
                    spatialAlignmentObjectParent = null;
                }
            }
        }

        #endregion IMixedRealityToolkitService Implementation

        #region IMixedRealitySpatialAlignmentSystem Implementation

        /// <summary>
        /// The collection of registered spatial coordinate observers.
        /// </summary>
        private List<ISpatialCoordinateObserver> observers = new List<ISpatialCoordinateObserver>();

        /// <summary>
        /// The parent object, in the hierarchy, under which all observed game objects will be placed.
        /// </summary>
        private GameObject spatialAlignmentObjectParent = null;

        /// <inheritdoc />
        public GameObject SpatialAlignmentObjectParent => spatialAlignmentObjectParent != null ? spatialAlignmentObjectParent : (spatialAlignmentObjectParent = CreateSpatialAlignmentObjectParent);

        /// <summary>
        /// Creates the parent for spatial alignment objects so that the scene hierarchy does not get overly cluttered.
        /// </summary>
        /// <returns>
        /// The <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see> to which spatial alignment objects will be parented.
        /// </returns>
        private GameObject CreateSpatialAlignmentObjectParent
        {
            get
            {
                GameObject newParent = new GameObject("Spatial Alignment System");
                MixedRealityPlayspace.AddChild(newParent.transform);

                return newParent;
            }
        }

        /// <inheritdoc />
        public GameObject CreateSpatialAlignmentObservationParent(string name)
        {
            GameObject objectParent = new GameObject(name);

            objectParent.transform.parent = SpatialAlignmentObjectParent.transform;

            return objectParent;
        }

        private uint nextSourceId = 0;

        /// <inheritdoc />
        public uint GenerateNewSourceId()
        {
            return nextSourceId++;
        }

        private SpatialAlignmentSystemProfile spatialAlignmentSystemProfile = null;

        /// <inheritdoc/>
        public SpatialAlignmentSystemProfile SpatialAlignmentSystemProfile
        {
            get
            {
                if (spatialAlignmentSystemProfile == null)
                {
                    spatialAlignmentSystemProfile = ConfigurationProfile as SpatialAlignmentSystemProfile;
                }
                return spatialAlignmentSystemProfile;
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<ISpatialCoordinateObserver> GetObservers()
        {
            return GetDataProviders() as IReadOnlyList<ISpatialCoordinateObserver>;
        }

        /// <inheritdoc />
        public IReadOnlyList<IMixedRealityDataProvider> GetDataProviders()
        {
            return new List<ISpatialCoordinateObserver>(observers) as IReadOnlyList<ISpatialCoordinateObserver>;
        }

        /// <inheritdoc />
        public IReadOnlyList<T> GetObservers<T>() where T : ISpatialCoordinateObserver
        {
            return GetDataProviders<T>();
        }


        /// <inheritdoc />
        public IReadOnlyList<T> GetDataProviders<T>() where T : IMixedRealityDataProvider
        {
            List<T> selected = new List<T>();

            for (int i = 0; i < observers.Count; i++)
            {
                if (observers[i] is T)
                {
                    selected.Add((T)observers[i]);
                }
            }

            return selected;
        }

        /// <inheritdoc />
        public ISpatialCoordinateObserver GetObserver(string name)
        {
            return GetDataProvider(name) as ISpatialCoordinateObserver;
        }

        /// <inheritdoc />
        public IMixedRealityDataProvider GetDataProvider(string name)
        {
            for (int i = 0; i < observers.Count; i++)
            {
                if (observers[i].Name == name)
                {
                    return observers[i];
                }
            }

            return null;
        }

        /// <inheritdoc />
        public T GetObserver<T>(string name = null) where T : ISpatialCoordinateObserver
        {
            return GetDataProvider<T>(name);
        }

        /// <inheritdoc />
        public T GetDataProvider<T>(string name = null) where T : IMixedRealityDataProvider
        {
            for (int i = 0; i < observers.Count; i++)
            {
                if (observers[i] is T)
                {
                    if ((name == null) || (observers[i].Name == name))
                    {
                        return (T)observers[i];
                    }
                }
            }

            return default(T);
        }

        /// <inheritdoc />
        public void ResumeObservers()
        {
            for (int i = 0; i < observers.Count; i++)
            {
                observers[i].Resume();
            }
        }

        /// <inheritdoc />
        public void ResumeObservers<T>() where T : ISpatialCoordinateObserver
        {
            for (int i = 0; i < observers.Count; i++)
            {
                if (observers[i] is T)
                {
                    observers[i].Resume();
                }
            }
        }

        /// <inheritdoc />
        public void ResumeObserver<T>(string name) where T : ISpatialCoordinateObserver
        {
            for (int i = 0; i < observers.Count; i++)
            {
                if ((observers[i] is T) && (observers[i].Name == name))
                {
                    observers[i].Resume();
                    break;
                }
            }
        }

        /// <inheritdoc />
        public void SuspendObservers()
        {
            for (int i = 0; i < observers.Count; i++)
            {
                observers[i].Suspend();
            }
        }

        /// <inheritdoc />
        public void SuspendObservers<T>() where T : ISpatialCoordinateObserver
        {
            for (int i = 0; i < observers.Count; i++)
            {
                if (observers[i] is T)
                {
                    observers[i].Suspend();
                }
            }
        }

        /// <inheritdoc />
        public void SuspendObserver<T>(string name) where T : ISpatialCoordinateObserver
        {
            for (int i = 0; i < observers.Count; i++)
            {
                if ((observers[i] is T) && (observers[i].Name == name))
                {
                    observers[i].Suspend();
                    break;
                }
            }
        }

        /// <inheritdoc />
        public void ClearObservations()
        {
            for (int i = 0; i < observers.Count; i++)
            {
                observers[i].ClearObservations();
            }
        }

        /// <inheritdoc />
        public void ClearObservations<T>(string name) where T : ISpatialCoordinateObserver
        {
            T observer = GetObserver<T>(name);
            observer?.ClearObservations();
        }

        /// <inheritdoc />
        public void RaiseCoordinateAdded(ISpatialCoordinateObserver observer, ISpatialCoordinate coordinate)
        {
            coordinateEventData.Initialize(observer, coordinate.Id, coordinate);
            HandleEvent(coordinateEventData, OnCoordinateAdded);
        }

        /// <summary>
        /// Event sent whenever a coordinate is added.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<ISpatialCoordinateHandler<ISpatialCoordinate>> OnCoordinateAdded =
            delegate (ISpatialCoordinateHandler<ISpatialCoordinate> handler, BaseEventData eventData)
        {
            SpatialCoordinateEventData<ISpatialCoordinate> spatialEventData = ExecuteEvents.ValidateEventData<SpatialCoordinateEventData<ISpatialCoordinate>>(eventData);
            handler.OnCoordinateAdded(spatialEventData);
        };

        /// <inheritdoc />
        public void RaiseCoordinateUpdated(ISpatialCoordinateObserver observer, ISpatialCoordinate coordinate)
        {
            coordinateEventData.Initialize(observer, coordinate.Id, coordinate);
            HandleEvent(coordinateEventData, OnCoordinateUpdated);
        }

        /// <summary>
        /// Event sent whenever a coordinate is updated.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<ISpatialCoordinateHandler<ISpatialCoordinate>> OnCoordinateUpdated =
            delegate (ISpatialCoordinateHandler<ISpatialCoordinate> handler, BaseEventData eventData)
        {
            SpatialCoordinateEventData<ISpatialCoordinate> spatialEventData = ExecuteEvents.ValidateEventData<SpatialCoordinateEventData<ISpatialCoordinate>>(eventData);
            handler.OnCoordinateUpdated(spatialEventData);
        };


        /// <inheritdoc />
        public void RaiseCoordinateRemoved(ISpatialCoordinateObserver observer, string coordinateId)
        {
            coordinateEventData.Initialize(observer, coordinateId, null);
            HandleEvent(coordinateEventData, OnCoordinateRemoved);
        }

        /// <summary>
        /// Event sent whenever a coordinate is discarded.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<ISpatialCoordinateHandler<ISpatialCoordinate>> OnCoordinateRemoved =
            delegate (ISpatialCoordinateHandler<ISpatialCoordinate> handler, BaseEventData eventData)
        {
            SpatialCoordinateEventData<ISpatialCoordinate> spatialEventData = ExecuteEvents.ValidateEventData<SpatialCoordinateEventData<ISpatialCoordinate>>(eventData);
            handler.OnCoordinateRemoved(spatialEventData);
        };

        #endregion IMixedRealitySpatialAlignmentSystem Implementation
    }
}
