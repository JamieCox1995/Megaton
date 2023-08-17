using System;
using System.Collections.Generic;
using System.Linq;
using TotalDistraction.ServiceLocation.Exceptions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace TotalDistraction.ServiceLocation
{
    /// <summary>
    /// Provides methods to register and automatically locate services.
    /// </summary>
    public static class ServiceLocator
    {
        /// <summary>
        /// Maps between the service interfaces (derived from <see cref="IUnityService"/>) and their implementations.
        /// </summary>
        private static Dictionary<Type, Type> ServiceTypes;
        /// <summary>
        /// Maps between the service implementation types and the instances of those service types.
        /// </summary>
        private static Dictionary<Type, IUnityService> Services;
        /// <summary>
        /// Maps between the service interfaces (derived from <see cref="IUnityService"/>) and the scenes for which they are active.
        /// A value of <code>null</code> indicates that the service should run for the entire lifetime of the game. 
        /// </summary>
        private static Dictionary<Type, string[]> ServiceLifetimes;
        /// <summary>
        /// Maps between the service interfaces (derived from <see cref="IUnityService"/>) and their configuration objects.
        /// </summary>
        private static Dictionary<Type, ServiceConfiguration> Configurations;
        /// <summary>
        /// Maps between the service interfaces (derived from <see cref="IUnityService"/>) and their configuration methods.
        /// </summary>
        private static Dictionary<Type, Delegate> ConfigurationDelegates;
        /// <summary>
        /// The root <see cref="GameObject"/> to attach dynamically created services to.
        /// </summary>
        private static GameObject RootGameObject;
        /// <summary>
        /// Should be <c>true</c> if the lifetime services have been constructed, otherwise <c>false</c>.
        /// </summary>
        private static bool LifetimeServicesAreSetup;

        /// <summary>
        /// Initializes the static members of the <see cref="ServiceLocator"/> class.
        /// </summary>
        static ServiceLocator()
        {
            ServiceTypes = new Dictionary<Type, Type>();
            Services = new Dictionary<Type, IUnityService>();
            ServiceLifetimes = new Dictionary<Type, string[]>();
            Configurations = new Dictionary<Type, ServiceConfiguration>();
            ConfigurationDelegates = new Dictionary<Type, Delegate>();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>
        /// Locates a registered service.
        /// </summary>
        /// <typeparam name="T">The service interface to locate.</typeparam>
        /// <returns></returns>
        public static T Get<T>()
            where T : class, IUnityService
        {
            Type serviceType = typeof(T);

            if (!ServiceTypes.ContainsKey(serviceType)) throw new UnregisteredServiceException(serviceType);
            
            IUnityService result;

            if (Services.TryGetValue(serviceType, out result) && result != null)
            {
                return (T)result;
            }
            else
            {
                throw new ServiceNotFoundException(serviceType);
            }
        }

        /// <summary>
        /// Sets the provided object as the root <see cref="GameObject"/> to attach dynamically created services to.
        /// </summary>
        /// <param name="obj">The <see cref="GameObject"/> to set as the root object.</param>
        public static void SetRootGameObject(GameObject obj)
        {
            RootGameObject = obj;
            UnityObject.DontDestroyOnLoad(RootGameObject);
        }

        /// <summary>
        /// Registers the type <typeparamref name="TImpl"/> as the service to use when an instance of <typeparamref name="TService"/> is requested.
        /// This service will be accessible for the entire lifetime of the game.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <typeparam name="TImpl">The service implementation type.</typeparam>
        public static void Register<TService, TImpl>()
            where TService : class, IUnityService
            where TImpl : class, TService
        {
            RegisterForScenes<TService, TImpl>(null);
        }

        /// <summary>
        /// Registers the type <typeparamref name="TImpl"/> as the service to use when an instance of <typeparamref name="TService"/> is requested,
        /// using an automatically discovered resource of the type <typeparamref name="TConfiguration"/> to configure the service when it is initialized.
        /// This service will be accessible for the entire lifetime of the game.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <typeparam name="TImpl">The service implementation type.</typeparam>
        /// <typeparam name="TConfiguration"></typeparam>
        public static void Register<TService, TImpl, TConfiguration>()
            where TService : class, IUnityService<TConfiguration>
            where TImpl : class, TService
            where TConfiguration : ServiceConfiguration
        {
            RegisterForScenes<TService, TImpl, TConfiguration>(null);
        }

        /// <summary>
        /// Registers the type <typeparamref name="TImpl"/> as the service to use when an instance of <typeparamref name="TService"/> is requested.
        /// This service will be accessible only within the specified scenes, and has a lifetime synchronised to the scene.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <typeparam name="TImpl">The service implementation type.</typeparam>
        /// <param name="sceneNames">The scene names for which to make this service accessible.</param>
        public static void RegisterForScenes<TService, TImpl>(params string[] sceneNames)
            where TService : class, IUnityService
            where TImpl : class, TService
        {
            ServiceTypes[typeof(TService)] = typeof(TImpl);
            Services[typeof(TService)] = null;
            ServiceLifetimes[typeof(TService)] = sceneNames;
        }

        /// <summary>
        /// Registers the type <typeparamref name="TImpl"/> as the service to use when an instance of <typeparamref name="TService"/> is requested,
        /// using an automatically discovered resource of the type <typeparamref name="TConfiguration"/> to configure the service when it is initialized.
        /// This service will be accessible only within the specified scenes, and has a lifetime synchronised to the scene.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <typeparam name="TImpl">The service implementation type.</typeparam>
        /// <typeparam name="TConfiguration"></typeparam>
        /// <param name="sceneNames">The scene names for which to make this service accessible.</param>
        public static void RegisterForScenes<TService, TImpl, TConfiguration>(params string[] sceneNames)
            where TService : class, IUnityService<TConfiguration>
            where TImpl : class, TService
            where TConfiguration : ServiceConfiguration
        {
            try
            {
                Configurations[typeof(TService)] = GetConfigurationObject<TConfiguration>();
            }
            catch (InvalidOperationException ex)
            {
                throw new ServiceConfigurationException(typeof(TService), ex);
            }

            Action<TService, TConfiguration> action = (service, config) => service.Configure(config);
            ConfigurationDelegates[typeof(TService)] = action;

            RegisterForScenes<TService, TImpl>(sceneNames);
        }

        /// <summary>
        /// Executes when the <see cref="SceneManager.sceneLoaded"/> event fires.
        /// </summary>
        /// <param name="loadedScene">The scene that has just loaded.</param>
        /// <param name="mode">Specifies whether the scene was loaded in an additive manner or replaced the previous scene.</param>
        private static void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) return;

            // Enumerate all services and ensure they are running if they should be.
            foreach (var kvp in ServiceLifetimes)
            {
                var serviceInfo = new ServiceInfo(kvp.Key, kvp.Value);

                if (serviceInfo.IsLifetimeService)
                {
                    if (!LifetimeServicesAreSetup)
                    {
                        // Update the service locator's reference.
                        Services[serviceInfo.ServiceType] = GetServiceInstance(serviceInfo);
                    }
                    else
                    {
                        if (serviceInfo.IsImplementationMonoBehaviour)
                        {
                            IUnityService excludeFromDisable = Services[serviceInfo.ServiceType];
                            DisableMonoBehaviourServiceInHierarchy(serviceInfo, excludeFromDisable);
                        }
                        continue;
                    }
                }
                else
                {
                    if (serviceInfo.RegisteredScenes.Contains(loadedScene.name))
                    {
                        // Update the service locator's reference.
                        Services[serviceInfo.ServiceType] = GetServiceInstance(serviceInfo);
                    }
                    else
                    {
                        if (serviceInfo.IsImplementationMonoBehaviour) DisableMonoBehaviourServiceInHierarchy(serviceInfo);
                        continue;
                    }
                }
            }

            if (!LifetimeServicesAreSetup) LifetimeServicesAreSetup = true;
        }

        /// <summary>
        /// Executes when the <see cref="SceneManager.sceneUnloaded"/> event fires.
        /// </summary>
        /// <param name="unloadedScene">The scene that has just unloaded.</param>
        private static void OnSceneUnloaded(Scene unloadedScene)
        {
            // Enumerate all services and destroy those that are dynamically created, scene-lifetime, and MonoBehaviour-derived.
            foreach (var kvp in ServiceLifetimes)
            {
                var serviceInfo = new ServiceInfo(kvp.Key, kvp.Value);

                if (!serviceInfo.IsLifetimeService)
                {
                    if (serviceInfo.IsImplementationMonoBehaviour)
                    {
                        IUnityService service = Services[serviceInfo.ServiceType];
                        MonoBehaviour behaviour = (MonoBehaviour)service;

                        // If the service was dynamically created, destroy it.
                        if (behaviour != null && behaviour.transform.IsChildOf(RootGameObject.transform))
                        {
                            Services[serviceInfo.ServiceType] = null;
                            UnityObject.Destroy(behaviour.gameObject);
                        }
                    }
                    else
                    {
                        Services[serviceInfo.ServiceType] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Disables all instances of the service in the hierarchy if it is derived from a MonoBehaviour.
        /// </summary>
        /// <param name="serviceInfo">The object containing information about the service.</param>
        /// <param name="exclude">The service instance to exclude.</param>
        private static void DisableMonoBehaviourServiceInHierarchy(ServiceInfo serviceInfo, IUnityService exclude = null)
        {
            // Disable the monobehaviour in this scene.
            var behaviours = GameObject.FindObjectsOfType(serviceInfo.ImplementationType).Cast<MonoBehaviour>();

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (object.ReferenceEquals(behaviour, exclude)) continue;

                behaviour.enabled = false;
            }
        }

        /// <summary>
        /// Gets an instance of the specified service.
        /// </summary>
        /// <param name="serviceInfo">The object containing information about the service.</param>
        /// <returns></returns>
        private static IUnityService GetServiceInstance(ServiceInfo serviceInfo)
        {
            IUnityService serviceInstance;

            if (serviceInfo.IsImplementationMonoBehaviour)
            {
                // If the service implementation is a MonoBehaviour, it may have been attached to a GameObject in the scene.
                serviceInstance = GetOrCreateMonoBehaviourService(serviceInfo);
            }
            else
            {
                // Create a new instance directly using reflection.
                serviceInstance = (IUnityService)Activator.CreateInstance(serviceInfo.ImplementationType);
            }

            // If the service is configurable, configure it.
            ServiceConfiguration configuration;
            if (Configurations.TryGetValue(serviceInfo.ServiceType, out configuration))
            {
                try
                {
                    ConfigurationDelegates[serviceInfo.ServiceType].DynamicInvoke(serviceInstance, configuration);
                }
                catch (Exception ex)
                {
                    throw new ServiceConfigurationException(serviceInfo.ServiceType, ex);
                }
            }

            return serviceInstance;
        }

        /// <summary>
        /// Gets the service from the scene hierarchy, or creates it if none exists.
        /// </summary>
        /// <param name="serviceInfo">The object containing information about the service.</param>
        /// <returns></returns>
        private static IUnityService GetOrCreateMonoBehaviourService(ServiceInfo serviceInfo)
        {
            UnityObject[] objs = GameObject.FindObjectsOfType(serviceInfo.ImplementationType);

            int objCount = objs == null ? 0 : objs.Length;
            if (objCount > 1)
            {
                // If there are multiple instances of this service in the scene, then it is unclear which one should be returned
                // with a call to ServiceLocator.Get<T>(), so throw an exception.
                throw new ServiceInstanceCountException(serviceInfo.ServiceType, objCount);
            }
            else
            {
                IUnityService serviceInstance;

                if (objCount == 1)
                {
                    // Register the object in the scene as the service provider.
                    if (serviceInfo.IsLifetimeService) UnityObject.DontDestroyOnLoad(objs[0]);

                    serviceInstance = (IUnityService)objs[0];
                }
                else
                {
                    // Create a new object to be the service provider.
                    GameObject serviceObj = new GameObject(serviceInfo.ServiceType.Name);
                    serviceObj.transform.SetParent(RootGameObject.transform);

                    serviceInstance = (IUnityService)serviceObj.AddComponent(serviceInfo.ImplementationType);
                }

                return serviceInstance;
            }
        }

        private static T GetConfigurationObject<T>()
            where T : ServiceConfiguration
        {
            T result;
            try
            {
                result = Resources.LoadAll<T>(string.Empty).SingleOrDefault();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("More than one instance of the configuration type \"{0}\" was found.", ex);
            }

            if (result == null) throw new InvalidOperationException("No instances of the configuration type \"{0}\" were found.");

            return result;
        }

        /// <summary>
        /// Contains information about a service.
        /// </summary>
        private class ServiceInfo
        {
            /// <summary>
            /// Gets the service interface type.
            /// </summary>
            public Type ServiceType { get; private set; }
            /// <summary>
            /// Gets the service implementation type.
            /// </summary>
            public Type ImplementationType { get { return ServiceTypes[this.ServiceType]; } }
            /// <summary>
            /// Gets the scenes that the service is registered to, or <c>null</c> if the service is a lifetime service.
            /// </summary>
            public string[] RegisteredScenes { get; private set; }
            /// <summary>
            /// Gets whether the service is expected to be available for the entire lifetime of the game.
            /// </summary>
            public bool IsLifetimeService { get { return RegisteredScenes == null; } }
            /// <summary>
            /// Gets whether the implementing type of the service is derived from <see cref="MonoBehaviour"/>.
            /// If <c>true</c>, this service needs to be attached to a <see cref="GameObject"/> in order to function correctly.
            /// </summary>
            public bool IsImplementationMonoBehaviour { get { return typeof(MonoBehaviour).IsAssignableFrom(this.ImplementationType); } }

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceInfo"/> class.
            /// </summary>
            /// <param name="serviceType">The service interface type.</param>
            /// <param name="registeredScenes">The scenes that the service is registered to, or <c>null</c> if the service is a lifetime service.</param>
            public ServiceInfo(Type serviceType, string[] registeredScenes)
            {
                this.ServiceType = serviceType;
                this.RegisteredScenes = registeredScenes;
            }
        }
    }
}