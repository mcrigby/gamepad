﻿namespace CutilloRigby.Input.Gamepad;

public partial class GamepadController : IGamepadController
{
    private readonly string _deviceFile;
    private readonly IGamepadMapping _mapping;

    public IDictionary<byte, GamepadInput<short>> Axes { get; init; }
    public IDictionary<byte, GamepadInput<bool>> Buttons { get; init; }

    /// <summary>
    /// EventHandler to allow the notification of Button changes.
    /// </summary>
    public event EventHandler<GamepadInputEventArgs<bool>> ButtonChanged = delegate { };

    /// <summary>
    /// EventHandler to allow the notification of Axis changes.
    /// </summary>
    public event EventHandler<GamepadInputEventArgs<short>> AxisChanged = delegate { };

    public GamepadController(string deviceFile, IGamepadMapping mapping)
    {
        _deviceFile = deviceFile;
        _mapping = mapping;

        Axes = new Dictionary<byte, GamepadInput<short>>();
        Buttons = new Dictionary<byte, GamepadInput<bool>>();
    }

    public void Start(CancellationToken cancellationToken)
    {
        if (!File.Exists(_deviceFile))
        {
            throw new ArgumentException(nameof(_deviceFile), $"The device {_deviceFile} does not exist");
        }

        Task.Factory.StartNew(() => ProcessMessages(cancellationToken));
    }

    private void ProcessMessages(CancellationToken cancellationToken)
    {
        using (FileStream fs = new FileStream(_deviceFile, FileMode.Open))
        {
            byte[] message = new byte[8];

            while (!cancellationToken.IsCancellationRequested)
            {
                // Read chunks of 8 bytes at a time.
                fs.Read(message, 0, 8);

                if (message.HasConfiguration())
                {
                    ProcessConfiguration(message);
                }

                ProcessValues(message);
            }
        }
    }

    private void ProcessConfiguration(byte[] message)
    {
        byte key = message.GetAddress();

        if (message.IsButton())
        {
            if (!Buttons.ContainsKey(key))
                Buttons.Add(key, new GamepadInput<bool> {
                    Name = _mapping.GetButtonName(key),
                    Enabled = _mapping.GetButtonEnabled(key),
                    Value = _mapping.GetButtonDefaultValue(key)
                });
        }
        else if (message.IsAxis())
        {
            if (!Axes.ContainsKey(key))
                Axes.Add(key, new GamepadInput<short> {
                    Name = _mapping.GetAxisName(key),
                    Enabled = _mapping.GetAxisEnabled(key),
                    Value = _mapping.GetAxisDefaultValue(key)
                });
        }
    }

    private void ProcessValues(byte[] message)
    {
        byte address = message.GetAddress();

        if (message.IsButton())
        {
            var button = Buttons[address];
            var oldValue = button.Value;
            var newValue = message.IsButtonPressed();

            if (button.Enabled && oldValue != newValue)
            {
                button.Value = newValue;
                ButtonChanged?.Invoke(this, new GamepadInputEventArgs<bool> { 
                    Address = address, 
                    Name = button.Name, 
                    Value = button.Value 
                });
            }

            Buttons[address] = button;
        }
        else if (message.IsAxis())
        {
            var axis = Axes[address];
            var oldValue = axis.Value;
            var newValue = message.GetAxisValue();

            if (axis.Enabled && oldValue != newValue)
            {
                axis.Value = newValue;
                AxisChanged?.Invoke(this, new GamepadInputEventArgs<short> { 
                    Address = address, 
                    Name = axis.Name, 
                    Value = axis.Value 
                });
            }

            Axes[address] = axis;
        }
    }

    public void Dispose() { }
}
