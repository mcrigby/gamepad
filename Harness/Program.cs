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
            })
            .Build();

        var gamepad = host.Services.GetRequiredService<GamepadController>();

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
            Console.WriteLine($"Axis {e.Name} ({e.Address}) Changed: {e.Value}");
        };

        await host.StartAsync(lifetime.ApplicationStopping);
        Console.WriteLine("Start pushing the buttons/axis of your gamepad/joystick to see the output");
    }
}
