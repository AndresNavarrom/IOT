using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Observer;

public interface IDeviceObserver
{
    void update(IDevice device);
}
