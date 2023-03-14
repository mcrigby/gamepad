using System.Device.Pwm;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harness;

internal sealed class Steering_Servo : BackgroundService
{
    private const byte _channel = 1;

    private readonly IServoSettings _servoSettings;
    private readonly ILogger _logger;
    
    private readonly PwmChannel _steeringPwm;
    
    public Steering_Servo(IServoSettings servoSettings, ILogger<Steering_Servo> logger)
    {
        _servoSettings = servoSettings ?? throw new ArgumentNullException(nameof(servoSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _steeringPwm = PwmChannel.Create(0, _channel, 50, ServoMap.Value[0]);

        SetLogHandlers();
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _steeringPwm.Start();
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!_servoSettings.HasChannel(_channel))
            return;// Task.CompletedTask;

        byte lastSteering = 127;
        float dutyCycle = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            byte currentSteering = _servoSettings.GetChannel(_channel);
            if (lastSteering != currentSteering)
            {
                dutyCycle = ServoMap.Value[currentSteering];
                _steeringPwm.DutyCycle = dutyCycle;

                setInformation_Value(currentSteering, dutyCycle);
                lastSteering = currentSteering;
            }

            await Task.Delay(10);
        }

        _steeringPwm.DutyCycle = ServoMap.Value[0];

        return; // Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _steeringPwm.Stop();
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _steeringPwm.DutyCycle = ServoMap.Value[0];
        _steeringPwm.Stop();
        _steeringPwm.Dispose();

        base.Dispose();
    }

    private void SetLogHandlers()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            setInformation_Value = (value, dutyCycle) =>
                _logger.LogInformation("Steering Servo ({channel}) set to {value} (Duty Cycle: {dutyCycle:n3})", 
                    _channel, value, dutyCycle);
        }
    }

    private Action<byte, float> setInformation_Value = (value, dutyCycle) => { };
}
