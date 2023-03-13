namespace CutilloRigby.Input.Gamepad;

public sealed class GamepadAvailableEventArgs
{
    public bool Value { get; set; }

    public static readonly GamepadAvailableEventArgs Yes = new GamepadAvailableEventArgs { Value = true };
    public static readonly GamepadAvailableEventArgs No = new GamepadAvailableEventArgs { Value = false };
}
