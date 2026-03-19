using UMLIoT.Core.Controllers;
using UMLIoT.Core.Devices;
using UMLIoT.Core.Users;
using UMLIoT.Patterns.Command;
using UMLIoT.Patterns.Factory.Devices;

namespace UMLIoT.Patterns.Facade;

public class IoTFacade
{
    private readonly AuthService authService;
    private readonly UserRepository userRepository;
    private readonly ControladorIOT controladorIOT;

    public IoTFacade()
    {
        authService = new AuthService();
        userRepository = new UserRepository();
        controladorIOT = new ControladorIOT();
    }

    public bool login(string email, string password)
    {
        return authService.login(email, password);
    }

    public void logout(int userId)
    {
        authService.logout();
    }

    public IDevice? registerDevice(string type, string config)
    {
        DiviceCreator? creator = type.ToLowerInvariant() switch
        {
            "camera" => new CameraCreator(),
            "smartlight" => new SmartlightCreator(),
            "alarm" => new AlarmCreator(),
            _ => null
        };

        if (creator is null)
        {
            return null;
        }

        return controladorIOT.addDevice(creator);
    }

    public bool removeDevice(int deviceId)
    {
        return controladorIOT.removeDevice(deviceId);
    }

    public void turnOnDevice(int deviceId)
    {
        IDevice? device = controladorIOT.getAllDeviceInternal().FirstOrDefault(current => current is Device concrete && concrete.getId() == deviceId);
        if (device is ISwitchable switchable)
        {
            controladorIOT.executeCommand(new TurnOnCommand(switchable));
        }
    }

    public void turnOffDevice(int deviceId)
    {
        IDevice? device = controladorIOT.getAllDeviceInternal().FirstOrDefault(current => current is Device concrete && concrete.getId() == deviceId);
        if (device is ISwitchable switchable)
        {
            controladorIOT.executeCommand(new TurnOffCommand(switchable));
        }
    }

    public void activateAlarm(int deviceId)
    {
        IDevice? device = controladorIOT.getAllDeviceInternal().FirstOrDefault(current => current is Device concrete && concrete.getId() == deviceId);
        if (device is IAlarm alarm)
        {
            controladorIOT.executeCommand(new TriggerAlarmCommand(alarm));
        }
    }

    public void startRecording(int deviceId)
    {
        IDevice? device = controladorIOT.getAllDeviceInternal().FirstOrDefault(current => current is Device concrete && concrete.getId() == deviceId);
        if (device is IMonitorable monitorable)
        {
            controladorIOT.executeCommand(new StartRecordingCommand(monitorable));
        }
    }

    public string getDeviceStatus(int deviceId)
    {
        return controladorIOT.getDeviceStatus(deviceId);
    }

    public List<IDevice> getAllDevice()
    {
        return controladorIOT.getAllDeviceInternal();
    }
}
