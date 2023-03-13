namespace CutilloRigby.Input.Gamepad;

public sealed class GamepadAxisInput
{
    public string? Name { get; set; }
    public bool Enabled { get; set; }
    public short Value { get; set; }
    public short DefaultValue { get; set; }
}
