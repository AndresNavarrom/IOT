using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Factory.Devices;

public class AlarmCreator : DeviceCreator
{
    private readonly int id;
    private readonly string name;
    
    private readonly string ipAddress;

    public AlarmCreator(int id, string name, string ipAddress)
    {
        this.id = id;
        this.name = name;
        this.ipAddress = ipAddress;
    }

    public override IDevice DeviceCreatorMethod()
    {
        return new Alarm(id, name, ipAddress);
    }
}
