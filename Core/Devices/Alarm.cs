namespace UMLIoT.Core.Devices;

public class Alarm : Device, ISwitchable, IAlarm
{
    public Alarm(int id, string name, string ipAddress) : base(id, name, ipAddress)
        {
        }

    public void trigger()
        {
            connect();
            Console.WriteLine($"Alarm {name} triggered");
        }

    public void stop()
        {
            disconnect();
            Console.WriteLine($"Alarm {name} stopped");
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
