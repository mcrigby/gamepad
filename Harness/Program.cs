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
                services.AddSingleton<GamepadController>(provider => 
                {
                    var gamepadLogger = provider.GetRequiredService<ILogger<GamepadController>>();
                    // You should provide the gamepad file you want to connect to. /dev/input/js0 is the default
                    return new GamepadController("/dev/input/js0", new _8BitDoUltimateMapping(), 
                        gamepadLogger);
                });
                services.AddSingleton<IGamepadController, GamepadController>(provider => 
                    provider.GetRequiredService<GamepadController>()
                );
                services.AddHostedService<GamepadController>(provider =>
                    provider.GetRequiredService<GamepadController>()
                );
            })
            .Build();

        var gamepad = host.Services.GetRequiredService<GamepadController>();

        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(async () => await gamepad.StopAsync());

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

        host.RunAsync();            
        Console.WriteLine("Start pushing the buttons/axis of your gamepad/joystick to see the output");
        Console.ReadLine();
        // Remember to Dispose the GamepadController, so it can finish the Task that listens for changes in the gamepad
    }
}
