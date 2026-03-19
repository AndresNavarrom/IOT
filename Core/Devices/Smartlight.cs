using Schedule = System.Object;

namespace UMLIoT.Core.Devices;

public class Smartlight : Device, ISwitchable
{
    private string color;
    private Schedule schedule;

    public Smartlight(int id, string name, string ipAddress, string color, Schedule schedule) : base(id, name, ipAddress)
    {
    this.color = color;
    this.schedule = schedule;
    }

    public void setColor(string color)
    {
    this.color = color;
    }

    public void setSchedule()
    {
    schedule = $"Updated-{DateTime.Now:yyyyMMddHHmmss}";
    }

    public void turnOn()
    {
    connect();
    Console.WriteLine($"Smartlight {getName()} turned on with color {color}");
    }

    public void turnOff()
    {
    disconnect();
    Console.WriteLine($"Smartlight {getName()} turned off");
    }
}
