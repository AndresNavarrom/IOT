using Command = UMLIoT.Patterns.Command.ICommand;
using DeviceCreator = UMLIoT.Patterns.Factory.Devices.DiviceCreator;
using UMLIoT.Core.Devices;
using UMLIoT.Core.Users;

namespace UMLIoT.Core.Controllers;

public class ControladorIOT
{
    private readonly List<IDevice> devices;
    private readonly List<User> users;

    public ControladorIOT()
    {
        devices = new List<IDevice>();
        users = new List<User>();
    }

    public IDevice addDevice(DeviceCreator DeviceCreator)
    {
        IDevice device = DeviceCreator.DeviceCreatorMethod();
        devices.Add(device);
        return device;
    }

    public bool removeDevice(int id)
    {
        IDevice? device = devices.FirstOrDefault(current => current is Device concrete && concrete.getId() == id);
        if (device is null)
        {
            return false;
        }

        devices.Remove(device);
        return true;
    }

    public string getDeviceStatus(int id)
    {
        IDevice? device = devices.FirstOrDefault(current => current is Device concrete && concrete.getId() == id);
        return device?.getStatus().GetType().Name ?? string.Empty;
    }

    public void executeCommand(Command command)
    {
        command.execute();
    }

    public List<IDevice> getAllDeviceInternal()
    {
        return devices;
    }
}
