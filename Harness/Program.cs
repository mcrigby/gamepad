using System.Device.Gpio;
using CutilloRigby.Input.Gamepad;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harness;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseConsoleLifetime()
            .ConfigureHostOptions(options =>
            {
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                options.ShutdownTimeout = TimeSpan.FromSeconds(30);
            })
            .ConfigureLogging(builder => 
                builder.AddConsole()
            )
            .ConfigureServices(services =>
            {
                services.AddGamepadController("/dev/input/js0", new _8BitDoUltimateMapping());

                services.AddSingleton<GpioController>();
                services.AddSingleton<RCCarHat>();
            })
            .Build();

        var gamepad = host.Services.GetRequiredService<GamepadController>();
        var rcCarHat = host.Services.GetRequiredService<RCCarHat>();

        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(async () => await host.StopAsync());

        var braking = BrakingState.None;

        // Configure this if you want to get events when the state of a button changes
        gamepad.ButtonChanged += async (object? sender, GamepadInputEventArgs<bool> e) =>
        {
            rcCarHat.SetBlueLed(true);

            if (e.Address == 3 && !e.Value && braking == BrakingState.Braking)
            {
                rcCarHat.SetDrive(0);
                await Task.Delay(800);
                braking = BrakingState.Broke;
                rcCarHat.SetGreenLed(false);
                rcCarHat.SetDrive(MuxLTandRT(gamepad.Axes[5].Value, gamepad.Axes[4].Value));
            }
            else if (e.Address == 3 && e.Value)
            {
                braking = BrakingState.Braking;

                rcCarHat.SetGreenLed(true);

                if(MuxLTandRT(gamepad.Axes[5].Value, gamepad.Axes[4].Value) > TBLE01_Deadband_Upper)
                    rcCarHat.SetDrive(short.MinValue);
                else
                    rcCarHat.SetDrive(0);
            }
            else
                Console.WriteLine($"Button {e.Name} ({e.Address}) Changed: {e.Value}");

            rcCarHat.SetBlueLed(false);
        };
        // Configure this if you want to get events when the state of an axis changes
        gamepad.AxisChanged += async (object? sender, GamepadInputEventArgs<short> e) =>
        {
            rcCarHat.SetBlueLed(true);

            if (e.Address == 0)
                rcCarHat.SetSteering(e.Value);
            else if ((e.Address == 4 || e.Address == 5) && braking != BrakingState.Braking)
            {
                var driveVal = MuxLTandRT(gamepad.Axes[5].Value, gamepad.Axes[4].Value);

                if (driveVal < TBLE01_Deadband_Lower && braking == BrakingState.None)
                {
                    braking = BrakingState.Braking;
                    rcCarHat.SetGreenLed(true);

                    rcCarHat.SetDrive(short.MinValue);
                    await Task.Delay(200);

                    rcCarHat.SetDrive(0);
                    await Task.Delay(800);

                    rcCarHat.SetGreenLed(false);
                    braking = BrakingState.Broke;
                }
                else if (driveVal > TBLE01_Deadband_Upper)
                    braking = BrakingState.None;

                rcCarHat.SetDrive(MuxLTandRT(gamepad.Axes[5].Value, gamepad.Axes[4].Value));
            }
            else
                Console.WriteLine($"Axis {e.Name} ({e.Address}) Changed: {e.Value}");

            rcCarHat.SetBlueLed(false);
        };
        gamepad.AvailableChanged += (object? sender, GamepadAvailableEventArgs e) =>
        {
            rcCarHat.SetRedLed(!e.Value);
            rcCarHat.SetDrive(0);
            rcCarHat.SetSteering(0);
        };
        rcCarHat.ButtonChanged += (object sender, PinValueChangedEventArgs e) =>
        {
            if (e.ChangeType == PinEventTypes.Falling)
                rcCarHat.SetGreenLed(true);
            else
                rcCarHat.SetGreenLed(false);
        };

        rcCarHat.SetRedLed(!gamepad.IsAvailable);
        rcCarHat.SetDrive(0);
        rcCarHat.SetSteering(0);
        
        await host.StartAsync(lifetime.ApplicationStopping);
        await host.WaitForShutdownAsync(lifetime.ApplicationStopped);

        rcCarHat.SetRedLed(false);
        rcCarHat.SetGreenLed(false);
        rcCarHat.SetBlueLed(false);
    }

    private static short MuxLTandRT(short LT, short RT)
    {
        return (short)((RT - LT) / 2);
    }

    private const double TBLE01_Deadband_Lower = -1000; // Although 4% of 32768 is 1310
    private const double TBLE01_Deadband_Upper = 1000; // Although 4% of 32768 is 1310
}
