using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Observer;

public class SecuritySystem : IDeviceObserver
{
    public void update(IDevice device, string eventType)
    {
        string deviceType = device.GetType().Name;
        string status = device.getStatus().GetType().Name;

        if (eventType == DeviceEvents.AlarmTriggered || status == "ErrorStatus")
        {
            Console.WriteLine($"[SECURITY] CRITICAL ALERT on {deviceType}. Status={status} Event={eventType}");
            return;
        }

        if (status == "OfflineStatus" || eventType == DeviceEvents.TurnedOff || eventType == DeviceEvents.Removed)
        {
            Console.WriteLine($"[SECURITY] Notice: {deviceType} is not active. Status={status} Event={eventType}");
            return;
        }

        Console.WriteLine($"[SECURITY] {deviceType} verified. Status={status} Event={eventType}");
    }
}
