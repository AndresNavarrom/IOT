using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.State;

public class OnlineStatus : DeviceStatus
{
    public override void handleStatus(Device device)
    {
        Console.WriteLine("El dispositivo está funcionando correctamente.");

        
        //device.Connect();

        
        Console.WriteLine("Procesando datos...");
    }
}
