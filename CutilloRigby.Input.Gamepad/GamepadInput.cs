namespace CutilloRigby.Input.Gamepad;

public interface IGamepadInput<T>
{
    string Name { get; set; }
    bool NotifyChanged { get; set; }
    bool Enabled { get; set; }
    T Value { get; set; }
    T DefaultValue { get; set; }
}

public struct GamepadAxisInput : IGamepadInput<short>
{
    public string Name { get; set; }
    public bool NotifyChanged { get; set; }
    public bool Enabled { get; set; }
    public short Value { get; set; }
    public short DefaultValue { get; set; }
}

public struct GamepadButtonInput : IGamepadInput<bool>
{
    public string Name { get; set; }
    public bool NotifyChanged { get; set; }
    public bool Enabled { get; set; }
    public bool Value { get; set; }
    public bool DefaultValue { get; set; }
}
