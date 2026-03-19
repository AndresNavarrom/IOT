using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.State;

public class ErrorStatus : DeviceStatus
{
    public override void handleStatus(Device device)
    {
        Console.WriteLine("El dispositivo tiene un error.");

        Console.WriteLine("Revisando fallos...");

        //device.setStatus(new OfflineStatus());
    }
}
