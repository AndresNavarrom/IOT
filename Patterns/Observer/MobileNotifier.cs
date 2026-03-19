using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Observer;

public class MobileNotifier : IDeviceObserver
{
    public void update(IDevice device, string eventType)
    {
        string deviceType = device.GetType().Name;

        Console.WriteLine($"[MOBILE] Notification: {deviceType} -> {eventType}");
    }
}
