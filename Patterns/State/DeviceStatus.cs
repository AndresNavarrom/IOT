using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.State;

public abstract class DeviceStatus
{
    public abstract void handleStatus(Device device);
}
