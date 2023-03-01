namespace CutilloRigby.Input.Gamepad;

public class GamepadInputEventArgs<T>
{
    public byte Address { get; set; }
    public string? Name { get; set; }
    public T? Value { get; set; }
}
