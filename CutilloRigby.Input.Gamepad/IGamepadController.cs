namespace CutilloRigby.Input.Gamepad;

public interface IGamepadController : IDisposable
{
    IDictionary<byte, GamepadInput<short>> Axes { get; }
    IDictionary<byte, GamepadInput<bool>> Buttons { get; }

    event EventHandler<GamepadInputEventArgs<bool>> ButtonChanged;
    event EventHandler<GamepadInputEventArgs<short>> AxisChanged;

    void Start(CancellationToken cancellationToken);
}
