using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;

public class Startup : StartupBase
{
    protected override void RegisterServices()
    {
		// Register the default services.
        base.RegisterServices();

        // Use ServiceLocator.Register<TService, TImpl>() or
        // ServiceLocator.RegisterForScenes<TService, TImpl>(string[])
        // to register the services needed by the game.

        ServiceLocator.Register<IInputProxyService, TeamUtilityInputProxyService>();
        ServiceLocator.Register<IInputManager, TeamUtilityInputProxyService, InputManagerConfiguration>();
        ServiceLocator.Register<ITestConfigurableService, TestConfigurableService, TestConfiguration>();
    }
}