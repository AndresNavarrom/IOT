using Schedule = System.Object;

namespace UMLIoT.Core.Devices;

public class Smartlight : Device, ISwitchable
{
    private string color;
    private Schedule? schedule;

    public Smartlight()
    {
        color = string.Empty;
        schedule = null;
    }

    public void setColor(string color)
    {
        this.color = color;
    }

    public void setSchedule()
    {
        schedule = new object();
    }

    public void turnOn()
    {
    }

    public void turnOff()
    {
    }
}
