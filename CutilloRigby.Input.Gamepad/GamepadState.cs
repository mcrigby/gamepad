using Microsoft.Extensions.Logging;

namespace CutilloRigby.Input.Gamepad;

public sealed class GamepadState : IGamepadState, IGamepadInputChanged
{
    public GamepadState(ILogger<GamepadState> logger)
    {
        SetLogHandlers(logger ?? throw new ArgumentNullException(nameof(logger)));

        Name = Default_Name;
        DeviceFile = Default_DeviceFile;
        Axes = new Dictionary<byte, GamepadAxisInput>();
        Buttons = new Dictionary<byte, GamepadButtonInput>();
    }

    public string Name { get; set; }
    public string DeviceFile { get; set; }

    public IDictionary<byte, GamepadAxisInput> Axes { get; set; }
    public IDictionary<byte, GamepadButtonInput> Buttons { get; set; }

    public const string Default_Name = "Unknown";
    public const string Default_DeviceFile = "/dev/input/js0";

    public event EventHandler<GamepadButtonInputEventArgs> ButtonChanged = delegate { };
    public event EventHandler<GamepadAxisInputEventArgs> AxisChanged = delegate { };

    public bool HasAxis(byte address)
    {
        lock (Axes)
        {
            return Axes.ContainsKey(address);
        }
    }

    public bool HasButton(byte address)
    {
        lock (Buttons)
        {
            return Buttons.ContainsKey(address);
        }
    }

    public void SetAxis(byte address, short value)
    {
        lock (Axes)
        {
            if (!Axes.ContainsKey(address) || !Axes[address].Enabled)
                return;
        }

        if (GetAxis(address) != value)
        {
            GamepadAxisInputEventArgs eventArgs;

            lock (Axes)
            {
                var axis = Axes[address];
                    
                setInformation_ValueChanged("Axis", axis.Name, axis.Value, value);

                axis.Value = value;
                Axes[address] = axis;

                eventArgs = Axes[address].ToEventArgs(address);
            }

            AxisChanged?.Invoke(this, eventArgs);
        }
    }

    public void SetButton(byte address, bool value)
    {
        lock (Buttons)
        {
            if (!Buttons.ContainsKey(address) || !Buttons[address].Enabled)
                return;
        }

        if (GetButton(address) != value)
        {
            GamepadButtonInputEventArgs eventArgs;

            lock (Buttons)
            {
                var button = Buttons[address];

                setInformation_ValueChanged("Button", button.Name, button.Value, value);

                button.Value = value;
                Buttons[address] = button;

                eventArgs = Buttons[address].ToEventArgs(address);
            }

            ButtonChanged?.Invoke(this, eventArgs);
        }
    }

    public short GetAxis(byte address)
    {
        if (!Axes.ContainsKey(address))
            return 0;

        lock (Axes)
        {
            return Axes[address].Value;
        }
    }

    public bool GetButton(byte address)
    {
        if (!Buttons.ContainsKey(address))
            return false;

        lock (Buttons)
        {
            return Buttons[address].Value;
        }
    }

    public string GetAxisName(byte address)
    {
        if (!Axes.ContainsKey(address))
            return Default_Name;

        lock (Axes)
        {
            return Axes[address]?.Name ?? Default_Name;
        }
    }

    public string GetButtonName(byte address)
    {
        if (!Buttons.ContainsKey(address))
            return Default_Name;

        lock (Buttons)
        {
            return Buttons[address]?.Name ?? Default_Name;
        }
    }

    public void ResetAxes()
    {
        lock (Axes)
        {
            foreach (var address in Axes.Keys)
                Axes[address].Value = Axes[address].DefaultValue;
        }
    }

    public void ResetButtons()
    {
        lock (Buttons)
        {
            foreach (var address in Buttons.Keys)
                Buttons[address].Value = Buttons[address].DefaultValue;
        }
    }

    public bool HasChannel(byte address)
    {
        return address switch
        {
            0 => true,
            1 => true,
            _ => false,
        };
    }

    public void SetChannel(byte address, byte value) { }

    public byte GetChannel(byte address)
    {
        return address switch
        {
            0 => (byte)(((GetAxis(4) - GetAxis(5)) / 2) >> 8),
            1 => (byte)(GetAxis(0) >> 8),
            _ => 0,
        };
    }

    private void SetLogHandlers(ILogger logger)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            setInformation_ValueChanged = (type, name, oldValue, newValue) => 
                logger.LogInformation(type + " {name} value changed from {oldValue} to {newValue}.", 
                        name, oldValue, newValue);;
        }
    }

    private Action<string, string?, object?, object?> setInformation_ValueChanged = (type, name, oldValue, newValue) => { };
}
