namespace CutilloRigby.Input.Gamepad;

public struct GamepadInput<T>
{
    public string Name { get; set; }
    public T Value { get; set; }
}
