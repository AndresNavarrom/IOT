using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Factory.Devices;

public abstract class DeviceCreator
{
    public abstract IDevice DeviceCreatorMethod();
}
