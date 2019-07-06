// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.SpatialAlignment
{
    /// <summary>
    /// Class providing the default implementation of the <see cref="IMixedRealitySpatialAlignmentSystem"/> interface.
    /// </summary>
    [DocLink("https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/SpatialAlignment/SpatialAlignmentGettingStarted.html")]
    public class MixedRealitySpatialAlignmentSystem : BaseCoreSystem, IMixedRealitySpatialAlignmentSystem, IMixedRealityDataProviderAccess, IMixedRealityCapabilityCheck
    {
        public MixedRealitySpatialAlignmentSystem(
            IMixedRealityServiceRegistrar registrar,
            MixedRealitySpatialAlignmentSystemProfile profile) : base(registrar, profile)
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

        private MixedRealitySpatialAlignmentEventData<SpatialAlignmentCoordinateObject> coordinateEventData = null;

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
            coordinateEventData = new MixedRealitySpatialAlignmentEventData<SpatialAlignmentCoordinateObject>(EventSystem.current);

#if UNITY_EDITOR
            // No need to enable capabilities at this level, but perhaps at the provider leve. For example, QR Code.
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
                        Registrar.UnregisterDataProvider<IMixedRealitySpatialCoordinateObserver>(observers[i]);
                    }
                }
            }
            observers.Clear();
        }

        /// <inheritdoc/>
        public override void Enable()
        {
            base.Enable();

            MixedRealitySpatialAlignmentSystemProfile profile = ConfigurationProfile as MixedRealitySpatialAlignmentSystemProfile;

            if ((observers.Count == 0) && (profile != null))
            {
                // Register the spatial observers.
                for (int i = 0; i < profile.ObserverConfigurations.Length; i++)
                {
                    MixedRealityCoordinateObserverConfiguration configuration = profile.ObserverConfigurations[i];
                    object[] args = { Registrar, this, configuration.ComponentName, configuration.Priority, configuration.ObserverProfile };

                    if (Registrar.RegisterDataProvider<IMixedRealitySpatialCoordinateObserver>(
                        configuration.ComponentType.Type,
                        configuration.RuntimePlatform,
                        args))
                    {
                        observers.Add(Registrar.GetDataProvider<IMixedRealitySpatialCoordinateObserver>(configuration.ComponentName));
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
            // No need to disable capabilities at this level, but perhaps at the provider leve. For example, QR Code.
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
        /// The collection of registered spatial corrdinate observers.
        /// </summary>
        private List<IMixedRealitySpatialCoordinateObserver> observers = new List<IMixedRealitySpatialCoordinateObserver>();

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

            objectParent.transform.parent = SpatialAObjectParent.transform;

            return objectParent;
        }

        private uint nextSourceId = 0;

        /// <inheritdoc />
        public uint GenerateNewSourceId()
        {
            return nextSourceId++;
        }

        private MixedRealitySpatialAlignmentSystemProfile spatialAlignmentSystemProfile = null;

        /// <inheritdoc/>
        public MixedRealitySpatialAlignmentSystemProfile SpatialAlignmentSystemProfile
        {
            get
            {
                if (spatialAlignmentSystemProfile == null)
                {
                    spatialAlignmentSystemProfile = ConfigurationProfile as MixedRealitySpatialAlignmentSystemProfile;
                }
                return spatialAlignmentSystemProfile;
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<IMixedRealitySpatialCoordinateObserver> GetObservers()
        {
            return GetDataProviders() as IReadOnlyList<IMixedRealitySpatialCoordinateObserver>;
        }

        /// <inheritdoc />
        public IReadOnlyList<IMixedRealityDataProvider> GetDataProviders()
        {
            return new List<IMixedRealitySpatialCoordinateObserver>(observers) as IReadOnlyList<IMixedRealitySpatialCoordinateObserver>;
        }

        /// <inheritdoc />
        public IReadOnlyList<T> GetObservers<T>() where T : IMixedRealitySpatialCoordinateObserver
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
        public IMixedRealitySpatialCoordinateObserver GetObserver(string name)
        {
            return GetDataProvider(name) as IMixedRealitySpatialCoordinateObserver;
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
        public T GetObserver<T>(string name = null) where T : IMixedRealitySpatialCoordinateObserver
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
        public void ResumeObservers<T>() where T : IMixedRealitySpatialCoordinateObserver
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
        public void ResumeObserver<T>(string name) where T : IMixedRealitySpatialCoordinateObserver
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
        public void SuspendObservers<T>() where T : IMixedRealitySpatialCoordinateObserver
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
        public void SuspendObserver<T>(string name) where T : IMixedRealitySpatialCoordinateObserver
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
        public void ClearObservations<T>(string name) where T : IMixedRealitySpatialCoordinateObserver
        {
            T observer = GetObserver<T>(name);
            observer?.ClearObservations();
        }

        /// <inheritdoc />
        public void RaiseCoordinateAdded(IMixedRealitySpatialCoordinateObserver observer, int coordinateId, SpatialCoordinateObject coordinateObject)
        {
            coordinateEventData.Initialize(observer, coordinateId, coordinateObject);
            HandleEvent(coordinateEventData, OnCoordinateAdded);
        }

        /// <summary>
        /// Event sent whenever a coordinate is added.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<IMixedRealitySpatialCoordinateHandler<SpatialCoordinateObject>> OnCoordinateAdded =
            delegate (IMixedRealitySpatialCoordinateHandler<SpatialCoordinateObject> handler, BaseEventData eventData)
            {
                MixedRealitySpatialCoordinateEventData<SpatialCoordinateObject> spatialEventData = ExecuteEvents.ValidateEventData<MixedRealityCoordinateEventData<SpatialCoordinateObject>>(eventData);
                handler.OnCoordinateAdded(spatialEventData);
            };

        /// <inheritdoc />
        public void RaiseCoordinateUpdated(IMixedRealitySpatialCoordinateObserver observer, int coordinateId, SpatialCoordinateObject coordinateObject)
        {
            coordinateEventData.Initialize(observer, coordinateId, coordinateObject);
            HandleEvent(coordinateEventData, OnCoordinateUpdated);
        }

        /// <summary>
        /// Event sent whenever a coordinate is updated.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<IMixedRealitySpatialCoordinateHandler<SpatialCoordinateObject>> OnCoordinateUpdated =
            delegate (IMixedRealitySpatialCoordinateHandler<SpatialCoordinateObject> handler, BaseEventData eventData)
            {
                MixedRealityCoordinateEventData<SpatialCoordinateObject> spatialEventData = ExecuteEvents.ValidateEventData<MixedRealityCoordinateEventData<SpatialCoordinateObject>>(eventData);
                handler.OnCoordinateUpdated(spatialEventData);
            };


        /// <inheritdoc />
        public void RaiseCoordinateRemoved(IMixedRealitySpatialCoordinateObserver observer, int coordinateId)
        {
            coordinateEventData.Initialize(observer, coordinateId, null);
            HandleEvent(coordinateEventData, OnCoordinateRemoved);
        }

        /// <summary>
        /// Event sent whenever a coordinate is discarded.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<IMixedRealitySpatialCoordinateHandler<SpatialCoordinateObject>> OnCoordinateRemoved =
            delegate (IMixedRealitySpatialCoordinateHandler<SpatialCoordinateObject> handler, BaseEventData eventData)
            {
                MixedRealityCoordinateEventData<SpatialCoordinateObject> spatialEventData = ExecuteEvents.ValidateEventData<MixedRealityCoordinateEventData<SpatialCoordinateObject>>(eventData);
                handler.OnCoordinateRemoved(spatialEventData);
            };

        #endregion IMixedRealitySpatialAlignmentSystem Implementation
    }
}
