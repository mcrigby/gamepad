namespace CutilloRigby.Input.Gamepad;

public interface IGamepadInputEventArgs<T>
{
    public byte Address { get; set; }
    public string? Name { get; set; }
    public T? Value { get; set; }
}

public sealed class GamepadAxisInputEventArgs : IGamepadInputEventArgs<short>
{
    public byte Address { get; set; }
    public string? Name { get; set; }
    public short Value { get; set; }
}

public sealed class GamepadButtonInputEventArgs : IGamepadInputEventArgs<bool>
{
    public byte Address { get; set; }
    public string? Name { get; set; }
    public bool Value { get; set; }
}
