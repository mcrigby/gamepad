using CutilloRigby.Input.Gamepad;
using Microsoft.Extensions.Logging;

namespace Harness;

class Program
{
    static void Main(string[] args)
    {
        var _loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole()
        );

        var cancellationToken = new CancellationTokenSource();
        Console.CancelKeyPress += delegate {
            cancellationToken.Cancel();
        };

        // You should provide the gamepad file you want to connect to. /dev/input/js0 is the default
        using (IGamepadController gamepad = new GamepadController("/dev/input/js0", new _8BitDoUltimateMapping(), 
            _loggerFactory.CreateLogger<GamepadController>()))
        {
            gamepad.Start(cancellationToken.Token);
            
            Console.WriteLine("Start pushing the buttons/axis of your gamepad/joystick to see the output");

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

            Console.ReadLine();
            cancellationToken.Cancel();
        }
        // Remember to Dispose the GamepadController, so it can finish the Task that listens for changes in the gamepad
    }
}
