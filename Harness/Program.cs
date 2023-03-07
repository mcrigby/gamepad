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

        // Configure this if you want to get events when the state of a button changes
        gamepad.ButtonChanged += (object? sender, GamepadInputEventArgs<bool> e) =>
        {
            Console.WriteLine($"Button {e.Name} ({e.Address}) Changed: {e.Value}");
        };
        // Configure this if you want to get events when the state of an axis changes
        gamepad.AxisChanged += (object? sender, GamepadInputEventArgs<short> e) =>
        {
            rcCarHat.SetRedLed(true);

            if (e.Address == 0)
                rcCarHat.SetSteering(e.Value);
            else if (e.Address == 4 || e.Address == 5)
                rcCarHat.SetDrive(MuxLTandRT(gamepad.Axes[5].Value, gamepad.Axes[4].Value));
            else
                Console.WriteLine($"Axis {e.Name} ({e.Address}) Changed: {e.Value}");

            rcCarHat.SetRedLed(false);
        };
        rcCarHat.ButtonChanged += (object sender, PinValueChangedEventArgs e) =>
        {
            if (e.ChangeType == PinEventTypes.Falling)
                rcCarHat.SetBlueLed(true);
            else
                rcCarHat.SetBlueLed(false);
        };

        await host.StartAsync(lifetime.ApplicationStopping);
        await host.WaitForShutdownAsync(lifetime.ApplicationStopped);
    }

    private static short MuxLTandRT(short LT, short RT)
    {
        return (short)((RT - LT) / 2);
    }
}

