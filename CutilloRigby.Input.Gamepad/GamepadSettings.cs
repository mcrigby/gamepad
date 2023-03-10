using Microsoft.Extensions.Logging;

namespace CutilloRigby.Input.Gamepad;

public sealed class GamepadSettings : IGamepadSettings
{
    private readonly FileSystemWatcher _deviceFileWatcher;
    private readonly ILogger _logger;

    private FileInfo _deviceFile;
    private bool _isAvailable;

    public GamepadSettings(ILogger<GamepadSettings> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof (logger));

        _deviceFile = new FileInfo(Default_DeviceFile);
        _isAvailable = _deviceFile.Exists;

        Name = Default_Name;
        Axes = new Dictionary<byte, GamepadAxisInput>();
        Buttons = new Dictionary<byte, GamepadButtonInput>();

        _deviceFileWatcher = new FileSystemWatcher(_deviceFile.DirectoryName, _deviceFile.Name);
        _deviceFileWatcher.Created += (s, e) => IsAvailable = true;
        _deviceFileWatcher.Deleted += (s, e) => IsAvailable = false;
        _deviceFileWatcher.EnableRaisingEvents = true;
    }

    public string Name { get; set; }
    public string DeviceFile 
    { 
        get => _deviceFile.FullName;
        set
        {
            _deviceFile = new FileInfo(value);
            _deviceFileWatcher.Path = _deviceFile.DirectoryName;
            _deviceFileWatcher.Filter = _deviceFile.Name;
            IsAvailable = _deviceFile.Exists;
        }
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

    public IDictionary<byte, GamepadAxisInput> Axes { get; set; }
    public IDictionary<byte, GamepadButtonInput> Buttons { get; set; }

    public const string Default_Name = "Unknown";
    public const string Default_DeviceFile = "/dev/input/js0";

    public event EventHandler<GamepadButtonInputEventArgs> ButtonChanged = delegate { };
    public event EventHandler<GamepadAxisInputEventArgs> AxisChanged = delegate { };
    public event EventHandler<GamepadAvailableEventArgs> AvailableChanged = delegate { };

    public bool HasAxis(byte address)
    {
        return Axes.ContainsKey(address);
    }

    public bool HasButton(byte address)
    {
        return Buttons.ContainsKey(address);
    }

    public void AddAxis(byte address, GamepadAxisInput input)
    {
        if (!Axes.ContainsKey(address))
            Axes.Add(address, input);
    }

    public void AddButton(byte address, GamepadButtonInput input)
    {
        if (!Buttons.ContainsKey(address))
            Buttons.Add(address, input);
    }

    public void SetAxis(byte address, short value)
    {
        if (!Axes.ContainsKey(address) || !Axes[address].Enabled)
            return;

        if (Axes[address].Value != value)
        {
            var axis = Axes[address];

            if(_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Axis {name} value changed from {oldValue} to {newValue}.", 
                    axis.Name, axis.Value, value);

            axis.Value = value;
            Axes[address] = axis;

            if (Axes[address].NotifyChanged)
                AxisChanged?.Invoke(this, Axes[address].ToEventArgs(address));
        }
    }

    public void SetButton(byte address, bool value)
    {
        if (!Buttons.ContainsKey(address) || !Buttons[address].Enabled)
            return;

        if (Buttons[address].Value != value)
        {
            var button = Buttons[address];

            if(_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Button {name} value changed from {oldValue} to {newValue}.", 
                    button.Name, button.Value, value);

            button.Value = value;
            Buttons[address] = button;

            if (Buttons[address].NotifyChanged)
                ButtonChanged?.Invoke(this, Buttons[address].ToEventArgs(address));
        }
    }

    public short GetAxis(byte address)
    {
        if (!Axes.ContainsKey(address))
            return 0;

        return Axes[address].Value;
    }

    public bool GetButton(byte address)
    {
        if (!Buttons.ContainsKey(address))
            return false;

        return Buttons[address].Value;
    }

    public string GetAxisName(byte address)
    {
        if (!Axes.ContainsKey(address))
            return Default_Name;

        return Axes[address].Name;
    }

    public string GetButtonName(byte address)
    {
        if (!Buttons.ContainsKey(address))
            return Default_Name;

        return Buttons[address].Name;
    }

    public void ResetAxes()
    {
        foreach (var address in Axes.Keys)
            SetAxis(address, Axes[address].DefaultValue);
    }

    public void ResetButtons()
    {
        foreach (var address in Buttons.Keys)
            SetButton(address, Buttons[address].DefaultValue);
    }
}