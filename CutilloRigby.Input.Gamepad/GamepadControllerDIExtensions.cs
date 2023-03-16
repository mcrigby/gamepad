using CutilloRigby.Input.Gamepad;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class GamepadControllerDIExtensions
{
    public static IServiceCollection AddGamepadController(this IServiceCollection services)
    {
        services.AddSingleton<GamepadController>();
        services.AddSingleton<IGamepadAvailable>(provider =>
            provider.GetRequiredService<GamepadController>()
        );
        services.AddHostedService<GamepadController>(provider =>
            provider.GetRequiredService<GamepadController>()
        );

        return services;
    }

    public static IServiceCollection AddGamepadState(this IServiceCollection services,
        string? name, string? deviceFile, IDictionary<byte, GamepadAxisInput>? axes,
        IDictionary<byte, GamepadButtonInput>? buttons)
    {
        services.AddSingleton<GamepadState>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GamepadState>>();

            var result = new GamepadState(logger);
            result.Name = name ?? GamepadState.Default_Name;
            result.DeviceFile = deviceFile ?? GamepadState.Default_DeviceFile;

            result.Axes = axes ?? new Dictionary<byte, GamepadAxisInput>();
            result.Buttons = buttons ?? new Dictionary<byte, GamepadButtonInput>();

            return result;
        });

        services.AddGamepadStateInterfaces();

        return services;
    }

    public static IServiceCollection AddGamepadState(this IServiceCollection services,
        string? name, string? deviceFile, IDictionary<string, GamepadAxisInput>? axes,
        IDictionary<string, GamepadButtonInput>? buttons)
    {
        if ((axes != null && axes.Keys.Any(x => !byte.TryParse(x, out _)))
            || (buttons != null && buttons.Keys.Any(x => !byte.TryParse(x, out _))))
            throw new ArgumentException("Keys for Axes and Buttons must be parsable to byte.");

        services.AddSingleton<GamepadState>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<GamepadState>>();

            var result = new GamepadState(logger);
            result.Name = name ?? GamepadState.Default_Name;
            result.DeviceFile = deviceFile ?? GamepadState.Default_DeviceFile;

            result.Axes = axes?
                .ToDictionary(x => byte.Parse(x.Key), x => x.Value)
                ?? new Dictionary<byte, GamepadAxisInput>();
            result.Buttons = buttons?
                .ToDictionary(x => byte.Parse(x.Key), x => x.Value)
                ?? new Dictionary<byte, GamepadButtonInput>();

            return result;
        });

        services.AddGamepadStateInterfaces();

        return services;
    }

    private static IServiceCollection AddGamepadStateInterfaces(this IServiceCollection services)
    {
        services.AddSingleton<IGamepadState>(provider =>
            provider.GetRequiredService<GamepadState>()
        );
        services.AddSingleton<IGamepadInputChanged>(provider =>
            provider.GetRequiredService<GamepadState>()
        );

        return services;
    }
}
