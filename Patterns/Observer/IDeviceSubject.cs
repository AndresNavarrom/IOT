using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Observer;

public interface IDeviceSubject
{
    void addObserver(IDeviceObserver observer);
    void removeObserver(IDeviceObserver observer);
    void notifyObservers(IDevice device);
}
