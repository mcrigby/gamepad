using CutilloRigby.Input.Gamepad;

namespace Microsoft.Extensions.DependencyInjection;

public static class GamepadControllerDIExtensions
{
    public static IServiceCollection AddGamepadController(this IServiceCollection services)
    {
        services.AddSingleton<GamepadController>();
        services.AddHostedService<GamepadController>(provider =>
            provider.GetRequiredService<GamepadController>()
        );

        return services;
    }
}
