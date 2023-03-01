namespace CutilloRigby.Input.Gamepad;

internal static class GamepadInputExtensions
{
    public static GamepadInputEventArgs<T> ToEventArgs<T>(this GamepadInput<T> input, byte address)
    {
        return new GamepadInputEventArgs<T> { 
            Address = address, 
            Name = input.Name, 
            Value = input.Value 
        };
    }

    public static GamepadInput<bool> ToButtonInput(this byte address, IGamepadMapping mapping)
    {
        return new GamepadInput<bool> {
            Name = mapping.GetButtonName(address),
            Enabled = mapping.GetButtonEnabled(address),
            Value = mapping.GetButtonDefaultValue(address)
        };
    }

    public static GamepadInput<short> ToAxisInput(this byte address, IGamepadMapping mapping)
    {
        return new GamepadInput<short> {
            Name = mapping.GetAxisName(address),
            Enabled = mapping.GetAxisEnabled(address),
            Value = mapping.GetAxisDefaultValue(address)
        };
    }
}