namespace CutilloRigby.Input.Gamepad;

public sealed class GamepadAxisInputEventArgs
{
    public byte Address { get; set; }
    public string? Name { get; set; }
    public short Value { get; set; }
}
