using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.State;

public class OfflineStatus : DeviceStatus
{
    public override void handleStatus(Device device)
    {
        Console.WriteLine("El dispositivo se apagara en 5 segundos.");


        //device.Disconnect();


        Console.WriteLine("Dispositivo apagado...");
    }
}
