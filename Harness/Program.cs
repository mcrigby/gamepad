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
                options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
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

                services.AddSingleton<IGamepadSettings>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<GamepadSettings>>();
                    return gamepadSettingsConfiguration.ToGamepadSettings(logger);
                });
                services.AddSingleton<IGamepadInputChanged>(provider =>
                {
                    var iGamepadSettings = provider.GetRequiredService<IGamepadSettings>();
                    if (iGamepadSettings is IGamepadInputChanged iGamepadInputChanged)
                        return iGamepadInputChanged;
                    throw new InvalidCastException("GamepadSettings is not an IGamepadInputChanged.");
                });

                services.AddGamepadController();

                services.AddSingleton<GpioController>();
                services.AddSingleton<RCCarHat>();
            })
            .Build();


        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(async () => await host.StopAsync());

        Start(host);

        await host.StartAsync(lifetime.ApplicationStopping);
        await host.WaitForShutdownAsync(lifetime.ApplicationStopped);

        Stop(host);
    }

    private static short MuxLTandRT(short LT, short RT)
    {
        return (short)((RT - LT) / 2);
    }

    private static void Start(IHost? host)
    {
        if (host == null)
            return;

        var gamepadSettings = host.Services.GetRequiredService<IGamepadSettings>();
        var gamepadInputChanged = host.Services.GetRequiredService<IGamepadInputChanged>();
        var rcCarHat = host.Services.GetRequiredService<RCCarHat>();

        var braking = BrakingState.None;

        // Configure this if you want to get events when the state of a button changes
        gamepadInputChanged.ButtonChanged += async (object? sender, GamepadButtonInputEventArgs e) =>
        {
            rcCarHat.SetBlueLed(true);
        
            if (e.Address == 3 && !e.Value && braking == BrakingState.Braking)
            {
                rcCarHat.SetDrive(0);
                await Task.Delay(800);
                braking = BrakingState.None;
                rcCarHat.SetGreenLed(false);
                rcCarHat.SetDrive(MuxLTandRT(gamepadSettings.GetAxis(5), gamepadSettings.GetAxis(4)));
            }
            else if (e.Address == 3 && e.Value)
            {
                braking = BrakingState.Braking;
        
                rcCarHat.SetGreenLed(true);
        
                if(MuxLTandRT(gamepadSettings.GetAxis(5), gamepadSettings.GetAxis(4)) > TBLE01_Deadband_Upper)
                    rcCarHat.SetDrive(short.MinValue);
                else
                    rcCarHat.SetDrive(0);
            }
            else
                Console.WriteLine($"Button {e.Name} ({e.Address}) Changed: {e.Value}");
        
            rcCarHat.SetBlueLed(false);
        };
        // Configure this if you want to get events when the state of an axis changes
        gamepadInputChanged.AxisChanged += async (object? sender, GamepadAxisInputEventArgs e) =>
        {
            rcCarHat.SetBlueLed(true);
        
            if (e.Address == 0)
                rcCarHat.SetSteering(e.Value);
            else if ((e.Address == 4 || e.Address == 5) && braking != BrakingState.Braking)
            {
                var driveVal = MuxLTandRT(gamepadSettings.GetAxis(5), gamepadSettings.GetAxis(4));
        
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
        
                rcCarHat.SetDrive(MuxLTandRT(gamepadSettings.GetAxis(5), gamepadSettings.GetAxis(4)));
            }
            else
                Console.WriteLine($"Axis {e.Name} ({e.Address}) Changed: {e.Value}");
        
            rcCarHat.SetBlueLed(false);
        };
        gamepadInputChanged.AvailableChanged += (object? sender, GamepadAvailableEventArgs e) =>
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

        rcCarHat.SetRedLed(!gamepadSettings.IsAvailable);
        rcCarHat.SetDrive(0);
        rcCarHat.SetSteering(0);
    }

    private static void Stop(IHost? host)
    {
        if (host == null)
            return;

        var rcCarHat = host.Services.GetRequiredService<RCCarHat>();

        rcCarHat.SetRedLed(false);
        rcCarHat.SetGreenLed(false);
        rcCarHat.SetBlueLed(false);
    }

    private const double TBLE01_Deadband_Lower = -1000; // Although 4% of 32768 is 1310
    private const double TBLE01_Deadband_Upper = 1000; // Although 4% of 32768 is 1310
}
