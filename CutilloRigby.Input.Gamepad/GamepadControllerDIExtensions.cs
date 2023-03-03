using CutilloRigby.Input.Gamepad;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class GamepadControllerDIExtensions
{
    public static IServiceCollection AddGamepadController(this IServiceCollection services,
        string deviceFile, IGamepadMapping gamepadMapping)
    {
        services.AddSingleton<GamepadController>(provider => 
        {
            var gamepadLogger = provider.GetRequiredService<ILogger<GamepadController>>();
            // You should provide the gamepad file you want to connect to. /dev/input/js0 is the default
            return new GamepadController(deviceFile, gamepadMapping, 
                gamepadLogger);
        });
        services.AddSingleton<IGamepadController, GamepadController>(provider => 
            provider.GetRequiredService<GamepadController>()
        );
        services.AddHostedService<GamepadController>(provider =>
            provider.GetRequiredService<GamepadController>()
        );

        return services;
    }
}
