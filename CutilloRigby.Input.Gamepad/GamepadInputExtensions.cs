namespace CutilloRigby.Input.Gamepad;

internal static class GamepadInputExtensions
{
    public static GamepadButtonInputEventArgs ToEventArgs(this GamepadButtonInput input, byte address)
    {
        return new GamepadButtonInputEventArgs { 
            Address = address, 
            Name = input.Name, 
            Value = input.Value 
        };
    }

    public static GamepadAxisInputEventArgs ToEventArgs(this GamepadAxisInput input, byte address)
    {
        return new GamepadAxisInputEventArgs { 
            Address = address, 
            Name = input.Name, 
            Value = input.Value 
        };
    }
}