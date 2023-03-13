using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CutilloRigby.Input.Gamepad;

public partial class GamepadController : BackgroundService
{
    private readonly IGamepadSettings _settings;
    private readonly ILogger _logger;

    public GamepadController(IGamepadSettings settings, ILogger<GamepadController> logger)
    {
        _settings = settings;
        _logger = logger;
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
                while (!_settings.IsAvailable && !cancellationToken.IsCancellationRequested)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning("Waiting for device at {deviceFile}.", _settings.DeviceFile);
                    
                    _settings.ResetAxes();
                    _settings.ResetButtons();

                    await Task.Delay(1000);
                }

                using (FileStream fs = new FileStream(_settings.DeviceFile, FileMode.Open))
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
                    _logger.LogError("Failed to read Device at {deviceFile}.", _settings.DeviceFile);
            }
        }
    }

    private void ProcessConfiguration(byte[] message)
    {
        byte key = message.GetAddress();

        if (message.IsButton())
        {
            if (_settings.HasButton(key) && _logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Button {key} with Name {name} is Available. Value = {value}.", 
                    key, _settings.GetButtonName(key), _settings.GetButton(key));
            }
            else if (!_settings.HasButton(key) && _logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Button {key} is Available but not handled in Gamepad Settings.", 
                    key);
            }
        }
        else if (message.IsAxis())
        {
            if (_settings.HasAxis(key) && _logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Axis {key} with Name {name} is Available. Value = {value}.", 
                    key, _settings.GetAxisName(key), _settings.GetAxis(key));
            }
            else if (!_settings.HasAxis(key) && _logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Axis {key} is Available but not handled in Gamepad Settings.", 
                    key);
            }
        }
    }

    private void ProcessValues(byte[] message)
    {
        byte address = message.GetAddress();

        if (message.IsButton())
            _settings.SetButton(address, message.IsButtonPressed());
        else if (message.IsAxis())
            _settings.SetAxis(address, message.GetAxisValue());
    }
}
