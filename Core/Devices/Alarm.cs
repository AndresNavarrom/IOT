namespace UMLIoT.Core.Devices;

public class Alarm : Device, ISwitchable, IAlarm
{
    public void trigger()
    {
    }

    public void stop()
    {
    }

    public void turnOn()
    {
        trigger();
    }

    public void turnOff()
    {
        stop();
    }
}
