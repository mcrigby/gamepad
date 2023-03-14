namespace CutilloRigby.Input.Gamepad;

public interface IGamepadSettings
{
    public string Name { get; set; }
    public string DeviceFile { get; set; }

    bool HasAxis(byte address);
    bool HasButton(byte address);

    void SetAxis(byte address, short value);
    void SetButton(byte address, bool value);

    short GetAxis(byte address);
    bool GetButton(byte address);

    string GetAxisName(byte address);
    string GetButtonName(byte address);
    
    void ResetAxes();
    void ResetButtons();
}
