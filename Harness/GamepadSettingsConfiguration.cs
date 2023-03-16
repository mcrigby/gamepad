using CutilloRigby.Input.Gamepad;
using Microsoft.Extensions.Logging;

namespace Harness;

public sealed class GamepadSettingsConfiguration
{
    public string? Name { get; set; }
    public string? DeviceFile { get; set; }

    public IDictionary<string, GamepadAxisInput>? Axes { get; set; }
    public IDictionary<string, GamepadButtonInput>? Buttons { get; set; }
}