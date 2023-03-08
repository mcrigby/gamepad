namespace CutilloRigby.Input.Gamepad;

public interface IGamepadController : IDisposable
{
    bool IsAvailable { get; }

    IDictionary<byte, GamepadInput<short>> Axes { get; }
    IDictionary<byte, GamepadInput<bool>> Buttons { get; }

    event EventHandler<GamepadInputEventArgs<bool>> ButtonChanged;
    event EventHandler<GamepadInputEventArgs<short>> AxisChanged;
    event EventHandler<GamepadAvailableEventArgs> AvailableChanged;
}
