namespace CutilloRigby.Input.Gamepad;

public sealed class GamepadButtonInputEventArgs
{
    public byte Address { get; set; }
    public string? Name { get; set; }
    public bool Value { get; set; }
}
