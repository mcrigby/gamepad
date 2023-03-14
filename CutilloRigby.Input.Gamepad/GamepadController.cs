using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CutilloRigby.Input.Gamepad;

public partial class GamepadController : BackgroundService, IGamepadAvailable
{
    private readonly IGamepadSettings _settings;
    private readonly ILogger _logger;

    private readonly FileSystemWatcher _deviceFileWatcher;

    private bool _isAvailable = false;

    public GamepadController(IGamepadSettings settings, ILogger<GamepadController> logger)
    {
        _settings = settings;
        _logger = logger;

        var deviceFile = new FileInfo(_settings.DeviceFile);

        _isAvailable = deviceFile.Exists;

        _deviceFileWatcher = new FileSystemWatcher(deviceFile.DirectoryName ?? string.Empty, deviceFile.Name);
        _deviceFileWatcher.Created += (s, e) => IsAvailable = true;
        _deviceFileWatcher.Deleted += (s, e) => IsAvailable = false;
        _deviceFileWatcher.EnableRaisingEvents = true;

        AvailableChanged += delegate { };

        CreateLogHandlers();
    }

    public bool IsAvailable 
    {
        get => _isAvailable;
        private set
        {
            if (_isAvailable != value)
            {
                _isAvailable = value;
    
                if (_isAvailable)
                    AvailableChanged?.Invoke(this, GamepadAvailableEventArgs.Yes);
                else 
                    AvailableChanged?.Invoke(this, GamepadAvailableEventArgs.No);
            }
        }
    }

    public event EventHandler<GamepadAvailableEventArgs> AvailableChanged;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                while (!IsAvailable && !cancellationToken.IsCancellationRequested)
                {
                    _executeWarning_WaitingForDeviceFile();
                    
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
                        await fs.ReadAsync(message, 0, 8, cancellationToken); // To Do: Check if still blocking

                        if (message.HasConfiguration())
                        {
                            ProcessConfiguration(message);
                        }

                        ProcessValues(message);

                        await Task.Delay(10); // Seems to need this to switch between hosted services.
                    }
                }
            }
            catch (IOException)
            {
                _executeError_FailedToReadFromDeviceFile();
            }
        }
    }

    private void ProcessConfiguration(byte[] message)
    {
        var address = message.GetAddress();

        if (message.IsButton())
        {
            if (_settings.HasButton(address))
                _configurationInformation_Available("Button", address, _settings.GetButtonName(address), _settings.GetButton(address));
            else
                _configurationWarning_NotHandled("Button", address);
        }
        else if (message.IsAxis())
        {
            if (_settings.HasAxis(address))
                _configurationInformation_Available("Axis", address, _settings.GetAxisName(address), _settings.GetAxis(address));
            else
                _configurationWarning_NotHandled("Axis", address);
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

    private void CreateLogHandlers()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _configurationInformation_Available = (type, address, buttonName, value) =>
                _logger.LogInformation(type + " {key} with Name {name} is Available. Value = {value}.", address, buttonName, value);
        }

        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _configurationWarning_NotHandled = (type, address) =>
                _logger.LogWarning(type + " {key} is Available but not handled in Gamepad Settings.", address);

            _executeWarning_WaitingForDeviceFile = () => 
                _logger.LogWarning("Waiting for device at {deviceFile}.", _settings.DeviceFile);;
        }

        if (_logger.IsEnabled(LogLevel.Error))
        {
            _executeError_FailedToReadFromDeviceFile = () =>
                _logger.LogError("Failed to read Device at {deviceFile}.", _settings.DeviceFile);
        }
    }

    private Action<string, byte, string, object> _configurationInformation_Available = (type, address, buttonName, value) => { };
    private Action<string, byte> _configurationWarning_NotHandled = (type, address) => { };
    private Action _executeWarning_WaitingForDeviceFile = () => { };
    private Action _executeError_FailedToReadFromDeviceFile = () => { };
}
