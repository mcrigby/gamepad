namespace Harness;

public interface IServoSettings
{
    bool HasChannel(byte address);

    void SetChannel(byte address, byte value);

    byte GetChannel(byte address);
}