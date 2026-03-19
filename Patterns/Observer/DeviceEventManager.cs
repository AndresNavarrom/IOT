using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Observer;

public class DeviceEventManager : IDeviceSubject
{
    private readonly List<IDeviceObserver> observers;

    public DeviceEventManager()
    {
        observers = new List<IDeviceObserver>();
    }

    public void addObserver(IDeviceObserver observer)
    {
        observers.Add(observer);
    }

    public void removeObserver(IDeviceObserver observer)
    {
        observers.Remove(observer);
    }

    public void notifyObservers(IDevice device, string eventType)
    {
        foreach (IDeviceObserver observer in observers)
        {
            observer.update(device, eventType);
        }
    }
}
