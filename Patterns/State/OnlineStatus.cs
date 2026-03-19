using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.State;

public class OnlineStatus : DeviceStatus
{
    public override void handleStatus(Device device)
    {
        Console.WriteLine("El dispositivo esta funcionando correctamente.");
        Console.WriteLine("Procesando datos...");
    }
}
