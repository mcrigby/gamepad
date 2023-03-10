using CutilloRigby.Input.Gamepad;
using Microsoft.Extensions.Logging;

namespace Harness;

public sealed class GamepadSettingsConfiguration
{
    public string? Name { get; set; }
    public string? DeviceFile { get; set; }

    public IDictionary<string, GamepadAxisInput>? Axes { get; set; }
    public IDictionary<string, GamepadButtonInput>? Buttons { get; set; }

    public GamepadSettings ToGamepadSettings(ILogger<GamepadSettings> logger)
    {
        var result = new GamepadSettings(logger);
        result.Name = Name ?? GamepadSettings.Default_Name;
        result.DeviceFile = DeviceFile ?? GamepadSettings.Default_DeviceFile;

        result.Axes = Axes?
            .ToDictionary(x => byte.Parse(x.Key), x => x.Value)
            ?? new Dictionary<byte, GamepadAxisInput>();
        result.Buttons = Buttons?
            .ToDictionary(x => byte.Parse(x.Key), x => x.Value)
            ?? new Dictionary<byte, GamepadButtonInput>();

        return result;
    }
}