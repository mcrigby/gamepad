using CutilloRigby.Input.Gamepad;
using Microsoft.Extensions.Configuration;
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
                options.ShutdownTimeout = TimeSpan.FromSeconds(30);
            })
            .ConfigureLogging(builder => 
                builder.AddConsole()
            )
            .ConfigureHostConfiguration(configurationBuilder => {
                configurationBuilder
                    .AddJsonFile("./appsettings.json");
            })
            .ConfigureServices((hostBuilder, services) =>
            {
                var gamepadSettingsSection = hostBuilder.Configuration.GetSection("GamepadSettings");
                var gamepadSettingsConfiguration = gamepadSettingsSection.Get<GamepadSettingsConfiguration>(options => options.ErrorOnUnknownConfiguration = true);

                services.AddGamepadState(gamepadSettingsConfiguration.Name, gamepadSettingsConfiguration.DeviceFile,
                    gamepadSettingsConfiguration.Axes, gamepadSettingsConfiguration.Buttons);
                    
                services.AddGamepadController();
            })
            .Build();


        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(async () => await host.StopAsync());

        Start(host);

        await host.StartAsync(lifetime.ApplicationStopping);
        await host.WaitForShutdownAsync(lifetime.ApplicationStopped);
    }

    private static void Start(IHost? host)
    {
        if (host == null)
            return;

        var gamepadInputChanged = host.Services.GetRequiredService<IGamepadInputChanged>();
        var gamepadAvailable = host.Services.GetRequiredService<IGamepadAvailable>();

        gamepadAvailable.AvailableChanged += (s, e) => 
            Console.WriteLine($"Gamepad Availability Changed: {e.Value}");

        // Configure this if you want to get events when the state of a button changes
        gamepadInputChanged.ButtonChanged += (s, e) =>
            Console.WriteLine($"Button {e.Name} ({e.Address}) Changed: {e.Value}");
        // Configure this if you want to get events when the state of an axis changes
        gamepadInputChanged.AxisChanged += (s, e) =>
            Console.WriteLine($"Axis {e.Name} ({e.Address}) Changed: {e.Value}");

        // Do something else at the same time, to check gamepad isn't blocking
        Task.Run(() => {
            for (var i = 0; i < int.MaxValue; i++)
            {
                Console.WriteLine(i);
                Thread.Sleep(100);
            }
        });
    }
}
