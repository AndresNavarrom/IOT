using UMLIoT.Patterns.State;

namespace UMLIoT.Core.Devices;

public interface IDevice
{
    bool connect();
    bool disconnect();
    DeviceStatus getStatus();
}
