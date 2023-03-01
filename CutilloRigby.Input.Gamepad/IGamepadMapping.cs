namespace CutilloRigby.Input.Gamepad;

public interface IGamepadMapping 
{
    string GetAxisName(byte address);
    string GetButtonName(byte address);

    short GetAxisDefaultValue(byte address);
    bool GetButtonDefaultValue(byte address);

    bool GetAxisEnabled(byte address);
    bool GetButtonEnabled(byte address);
}
