using System.Device.Gpio;
using System.Device.Pwm;
using Microsoft.Extensions.Logging;

namespace Harness;

internal class RCCarHat : IDisposable
{
    private readonly GpioController _gpioController;
    private readonly ILogger _logger;
    
    private readonly PwmChannel _steeringPwm;
    private readonly PwmChannel _drivePwm;

    public event PinChangeEventHandler? ButtonChanged;

    private const int redLed = 22;
    private const int greenLed = 23;
    private const int blueLed = 24;

    private const int button = 20;

    public RCCarHat(GpioController gpioController, ILogger<RCCarHat> logger)
    {
        _gpioController = gpioController ?? throw new ArgumentNullException(nameof(gpioController));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _gpioController.OpenPin(redLed, PinMode.Output, PinValue.Low);
        _gpioController.OpenPin(greenLed, PinMode.Output, PinValue.Low);
        _gpioController.OpenPin(blueLed, PinMode.Output, PinValue.Low);

        _gpioController.OpenPin(button, PinMode.Input);
        _gpioController.RegisterCallbackForPinValueChangedEvent(button, PinEventTypes.Rising | PinEventTypes.Falling,
            ButtonCallback);

        _steeringPwm = PwmChannel.Create(0, 1, 50, ServoMap.Value[0]);
        _steeringPwm.Start();

        _drivePwm = PwmChannel.Create(0, 0, 50, ServoMap.Value[0]);
        _drivePwm.Start();
    }

    public void SetRedLed(bool value)
    {
        _gpioController.Write(redLed, value ? PinValue.High : PinValue.Low);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Red LED Set to {value}", value);
    }

    public void SetGreenLed(bool value)
    {
        _gpioController.Write(greenLed, value ? PinValue.High : PinValue.Low);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Green LED Set to {value}", value);
    }

    public void SetBlueLed(bool value)
    {
        _gpioController.Write(blueLed, value ? PinValue.High : PinValue.Low);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Blue LED Set to {value}", value);
    }

    public bool GetButton()
    {
        return _gpioController.Read(button) == PinValue.Low;
    }

    private void ButtonCallback(object sender, PinValueChangedEventArgs eventArgs)
    {
        var buttonEvent = ButtonChanged;
        buttonEvent?.Invoke(this, eventArgs);
    }

    public void SetSteering(short value)
    {
        var ordinal = (byte)(value >> 8);
        var dutyCycle = ServoMap.Value[ordinal];
        _steeringPwm.DutyCycle = dutyCycle;

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Steering Set to {value} (Duty Cycle: {dutyCycle:n3})", value, dutyCycle);
    }

    public void SetDrive(short value)
    {
        var ordinal = (byte)(value >> 8);
        var dutyCycle = ServoMap.Value[ordinal];
        _drivePwm.DutyCycle = dutyCycle;

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Drive Set to {value} (Duty Cycle: {dutyCycle:n3})", value, dutyCycle);
    }

    public void Dispose()
    {
        _gpioController.ClosePin(redLed);
        _gpioController.ClosePin(greenLed);
        _gpioController.ClosePin(blueLed);

        _gpioController.UnregisterCallbackForPinValueChangedEvent(button, ButtonCallback);
        _gpioController.ClosePin(button);

        _steeringPwm.Stop();
        _steeringPwm.Dispose();

        _drivePwm.Stop();
        _drivePwm.Dispose();
    }
}
