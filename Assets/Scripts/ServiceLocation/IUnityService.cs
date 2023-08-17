using UnityEngine;

namespace TotalDistraction.ServiceLocation
{
    /// <summary>
    /// Specifies that the class or interface defines or implements a service that can be registered with the
    /// <see cref="ServiceLocator"/>. This is a marker interface and has no methods.
    /// </summary>
    public interface IUnityService { }

    /// <summary>
    /// Specifies that the class or interface defines or implements a service that can be registered with the
    /// <see cref="ServiceLocator"/>.
    /// </summary>
    /// <typeparam name="TConfiguration">The type of the configuration.</typeparam>
    public interface IUnityService<TConfiguration> : IUnityService where TConfiguration : ServiceConfiguration
    {
        void Configure(TConfiguration configuration);
    }

    public abstract class ServiceConfiguration : ScriptableObject { }
}
