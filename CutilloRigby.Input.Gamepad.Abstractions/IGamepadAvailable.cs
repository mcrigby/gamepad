namespace CutilloRigby.Input.Gamepad;

public interface IGamepadAvailable
{
    bool IsAvailable { get; }
    event EventHandler<GamepadAvailableEventArgs> AvailableChanged;
}
