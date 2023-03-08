using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CutilloRigby.Input.Gamepad;

public partial class GamepadController : BackgroundService, IGamepadController
{
    private readonly string _deviceFile;
    private readonly IGamepadMapping _mapping;
    private readonly ILogger _logger;

    public bool IsAvailable => File.Exists(_deviceFile);

    public IDictionary<byte, GamepadInput<short>> Axes { get; init; }
    public IDictionary<byte, GamepadInput<bool>> Buttons { get; init; }

    public event EventHandler<GamepadInputEventArgs<bool>> ButtonChanged = delegate { };

    public event EventHandler<GamepadInputEventArgs<short>> AxisChanged = delegate { };

    public event EventHandler<GamepadAvailableEventArgs> AvailableChanged = delegate { };

    public GamepadController(string deviceFile, IGamepadMapping mapping, ILogger<GamepadController> logger)
    {
        _deviceFile = deviceFile;
        _mapping = mapping;
        _logger = logger;

        Axes = new Dictionary<byte, GamepadInput<short>>();
        Buttons = new Dictionary<byte, GamepadInput<bool>>();
    }

    public override Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                while (!IsAvailable && !cancellationToken.IsCancellationRequested)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning("Waiting for device at {deviceFile}.", _deviceFile);
                    await Task.Delay(5000);
                    if (IsAvailable)
                        AvailableChanged?.Invoke(this, GamepadAvailableEventArgs.Yes);
                }

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
            catch (IOException)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError("Device at {deviceFile} disconnected.", _deviceFile);
                
                AvailableChanged?.Invoke(this, GamepadAvailableEventArgs.No);
            }
        }
    }

    private void ProcessConfiguration(byte[] message)
    {
        byte key = message.GetAddress();

        if (message.IsButton())
        {
            if (!Buttons.ContainsKey(key))
            {
                var button = key.ToButtonInput(_mapping);
                Buttons.Add(key, button);

                if(_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Adding Button {key} with Name {name}. Value = {value}.", 
                        key, button.Name, button.Value);
            }
        }
        else if (message.IsAxis())
        {
            if (!Axes.ContainsKey(key))
            {
                var axis = key.ToAxisInput(_mapping);
                Axes.Add(key, axis);

                if(_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Adding Axis {key} with Name {name}. Value = {value}.", 
                        key, axis.Name, axis.Value);
            }
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
                if(_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Button {name} value changed from {oldValue} to {newValue}.", button.Name, oldValue, newValue);

                button.Value = newValue;
                Buttons[address] = button;
                ButtonChanged?.Invoke(this, button.ToEventArgs(address));
            }
        }
        else if (message.IsAxis())
        {
            var axis = Axes[address];
            var oldValue = axis.Value;
            var newValue = message.GetAxisValue();

            if (axis.Enabled && oldValue != newValue)
            {
                if(_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Axis {name} value changed from {oldValue} to {newValue}.", axis.Name, oldValue, newValue);

                axis.Value = newValue;
                Axes[address] = axis;
                AxisChanged?.Invoke(this, axis.ToEventArgs(address));
            }
        }
    }
}
