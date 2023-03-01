using CutilloRigby.Input.Gamepad;

namespace Harness;

internal sealed class _8BitDoUltimateMapping : IGamepadMapping
{
    public short GetAxisDefaultValue(byte address)
    {
        return address switch
        {
            4 => -32767,
            5 => -32767,
            _ => 0,
        };
    }

    public string GetAxisName(byte address)
    {
        return address switch
        {
            0 => "Left X",
            1 => "Left Y",
            2 => "Right X",
            3 => "Right Y",
            4 => "Right Trigger",
            5 => "Left Trigger",
            6 => "D-Pad X",
            7 => "D-Pad Y",
            _ => "Unknown",
        };
    }

    public bool GetButtonDefaultValue(byte address)
    {
        return address switch
        {
            _ => false,
        };
    }

    public string GetButtonName(byte address)
    {
        return address switch
        {
            0 => "A",
            1 => "B",
            3 => "X",
            4 => "Y",
            6 => "Left Shoulder",
            7 => "Right Shoulder",
            8 => "Left Trigger",
            9 => "Right Trigger",
            10 => "-",
            11 => "+",
            12 => "Home",
            19 => "Right Paddle",
            23 => "Left Paddle",
            _ => "Unknown",
        };
    }
}
