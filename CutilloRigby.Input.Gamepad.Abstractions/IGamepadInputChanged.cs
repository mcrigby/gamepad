namespace CutilloRigby.Input.Gamepad;

public interface IGamepadInputChanged
{
    event EventHandler<GamepadButtonInputEventArgs> ButtonChanged;
    event EventHandler<GamepadAxisInputEventArgs> AxisChanged;
    event EventHandler<GamepadAvailableEventArgs> AvailableChanged;
}
