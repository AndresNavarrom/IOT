using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Factory.Devices;

public class SmartlightCreator : DeviceCreator
{
    private readonly int id;
    private readonly string name;
    private DeviceStatus status;
    private readonly string ipAddress;
    private readonly string color;
    private readonly string schedule;

    public SmartlightCreator(int id, string name, string ipAddress, string color, string schedule, DeviceStatus status)
    {
        this.id = id;
        this.name = name;
        this.ipAddress = ipAddress;
        this.color = color;
        this.schedule = schedule;
        this.status = status;
    }

    public override IDevice DeviceCreatorMethod()
    {
        return new Smartlight(id, name, ipAddress, color, schedule, status);
    }
}
