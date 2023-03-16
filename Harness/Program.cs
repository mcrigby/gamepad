using System.Device.Gpio;
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

                //services.AddSingleton<IServoSettings>(provider =>
                //    provider.GetRequiredService<GamepadState>()
                //);

                services.AddGamepadState(gamepadSettingsConfiguration.Name, gamepadSettingsConfiguration.DeviceFile,
                    gamepadSettingsConfiguration.Axes, gamepadSettingsConfiguration.Buttons);
                    
                services.AddGamepadController();

                services.AddSingleton<GpioController>();
                services.AddSingleton<StatusLed>();

                services.AddHostedService<Steering_Servo>();
                services.AddHostedService<TBLE01_ESC>();
            })
            .Build();


        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(async () => await host.StopAsync());

        Start(host);

        await host.StartAsync(lifetime.ApplicationStopping);
        await host.WaitForShutdownAsync(lifetime.ApplicationStopped);

        Stop(host);
    }

    private static void Start(IHost? host)
    {
        if (host == null)
            return;

        var gamepadInputChanged = host.Services.GetRequiredService<IGamepadInputChanged>();
        var statusLed = host.Services.GetRequiredService<StatusLed>();

        // Configure this if you want to get events when the state of a button changes
        gamepadInputChanged.ButtonChanged += (object? sender, GamepadButtonInputEventArgs e) =>
        {
            statusLed.SetBlueLed(true);
        
            //if (e.Address == 3 && !e.Value && braking == DrivingMode.Braking)
            //{
            //    rcCarHat.SetDrive(0);
            //    await Task.Delay(800);
            //    braking = DrivingMode.ForwardOnly;
            //    statusLed.SetGreenLed(false);
            //    rcCarHat.SetDrive(MuxLTandRT(gamepadSettings.GetAxis(5), gamepadSettings.GetAxis(4)));
            //}
            //else if (e.Address == 3 && e.Value)
            //{
            //    braking = DrivingMode.Braking;
            //
            //    statusLed.SetGreenLed(true);
            //
            //    if(MuxLTandRT(gamepadSettings.GetAxis(5), gamepadSettings.GetAxis(4)) > TBLE01_Deadband_Upper)
            //        rcCarHat.SetDrive(short.MinValue);
            //    else
            //        rcCarHat.SetDrive(0);
            //}
            //else
                Console.WriteLine($"Button {e.Name} ({e.Address}) Changed: {e.Value}");
        
            statusLed.SetBlueLed(false);
        };
        // Configure this if you want to get events when the state of an axis changes
    }

    private static void Stop(IHost? host)
    {
        if (host == null)
            return;

        var statusLed = host.Services.GetRequiredService<StatusLed>();

        statusLed.SetRedLed(false);
        statusLed.SetGreenLed(false);
        statusLed.SetBlueLed(false);
    }
}
