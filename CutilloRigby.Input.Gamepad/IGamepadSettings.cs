namespace CutilloRigby.Input.Gamepad;

public interface IGamepadSettings
{
    public string Name { get; set; }
    public string DeviceFile { get; set; }
    bool IsAvailable { get; }

    bool HasAxis(byte address);
    bool HasButton(byte address);

    void AddAxis(byte address, GamepadAxisInput input);
    void AddButton(byte address, GamepadButtonInput input);

    void SetAxis(byte address, short value);
    void SetButton(byte address, bool value);

    short GetAxis(byte address);
    bool GetButton(byte address);

    string GetAxisName(byte address);
    string GetButtonName(byte address);
    
    void ResetAxes();
    void ResetButtons();
    
    event EventHandler<GamepadButtonInputEventArgs> ButtonChanged;
    event EventHandler<GamepadAxisInputEventArgs> AxisChanged;
    event EventHandler<GamepadAvailableEventArgs> AvailableChanged;
}
