namespace CutilloRigby.Input.Gamepad;

public sealed class GamepadButtonInput
{
    public string? Name { get; set; }
    public bool Enabled { get; set; }
    public bool Value { get; set; }
    public bool DefaultValue { get; set; }
}
