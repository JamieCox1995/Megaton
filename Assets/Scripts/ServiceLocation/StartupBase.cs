using System;
using System.Collections.Generic;
using System.Linq;
using TotalDistraction.ServiceLocation.DefaultServices;
using TotalDistraction.ServiceLocation.Exceptions;
using UnityEngine;

namespace TotalDistraction.ServiceLocation
{
    /// <summary>
    /// This class provides a framework for automatically registering services when the game starts.
    /// This class should be inherited from to define how service interfaces and implementations are managed.
    /// </summary>
    public abstract class StartupBase
    {
        /// <summary>
        /// Should be <c>true</c> once the service locator has been set up.
        /// </summary>
        private static bool IsServiceLocatorSetUp;

        /// <summary>
        /// Executes just before a scene is loaded.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnLoad()
        {
            if (!IsServiceLocatorSetUp)
            {
                IsServiceLocatorSetUp = true;

                ServiceLocator.SetRootGameObject(new GameObject("ServiceLocator"));

                // Reflection is used to create an instance of a derived type because the RuntimeInitializeOnLoadMethodAttribute
                // can only be attached to a static method, which cannot be marked as virtual or abstract. Additionally,
                // there should only be one type that derives from this class within the project to ensure that the behaviour is
                // the same each time this method executes, and using reflection makes it possible to perform that check.
                Type startupImplType;

                try
                {
                    startupImplType = FindAllDerivedTypes<StartupBase>().SingleOrDefault();
                }
                catch (InvalidOperationException ex)
                {
                    throw new StartupException(
                        "More than one class was found that derives from StartupBase. " +
                        "Ensure that only one class derives this class.",
                        ex);
                }

                if (startupImplType == null) throw new StartupException(
                    "No class was found that derives from StartupBase. " +
                    "This class must be subclassed to set up the service locator.");

                StartupBase startup = (StartupBase)Activator.CreateInstance(startupImplType);

                startup.RegisterServices();
            }
        }

        /// <summary>
        /// Gets all <see cref="Type"/> objects that are derived from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <returns></returns>
        private static IEnumerable<Type> FindAllDerivedTypes<T>()
        {
            Type baseType = typeof(T);

            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(t => t != baseType && baseType.IsAssignableFrom(t));
        }

        /// <summary>
        /// Registers services with the service locator.
        /// </summary>
        protected virtual void RegisterServices()
        {
            ServiceLocator.Register<IInputProxyService, InputProxyService>();
        }
    }
}
