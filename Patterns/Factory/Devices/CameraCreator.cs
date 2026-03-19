using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Factory.Devices;

public class CameraCreator : DeviceCreator
{
    private readonly int id;
    private readonly string name;

    private DeviceStatus status;
    private readonly string ipAddress;


    public CameraCreator(int id, string name, string ipAddress, DeviceStatus status)
    {
        this.id = id;
        this.name = name;
        this.ipAddress = ipAddress;
        this.status = status;
    }

    public override IDevice DeviceCreatorMethod()
    {
        return new Camera(id, name, ipAddress, status);
    }
}
