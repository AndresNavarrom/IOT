using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Observer;

public class EventLogger : IDeviceObserver
{
    public void update(IDevice device, string eventType)
    {
        string deviceType = device.GetType().Name;
        string status = device.getStatus().GetType().Name;

        Console.WriteLine($"[LOG] Event={eventType} Device={deviceType} Status={status}");
    }
}
