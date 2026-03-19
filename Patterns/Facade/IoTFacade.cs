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

    public IoTFacade(AuthService authService, UserRepository userRepository, ControladorIOT controladorIOT)
    {
        this.authService = authService;
        this.userRepository = userRepository;
        this.controladorIOT = controladorIOT;
    }

    public bool login(string email, string password)
    {
        return authService.login(email, password, password);
    }

    public void logout(int userId)
    {
        var user = userRepository.findById(userId);
        if (user is not null)
        {
            authService.logout();
        }
    }

    public IDevice? registerDevice(string type, object config)
    {
        if (config is not Dictionary<string, string> values)
        {
            return null;
        }

        var id = int.Parse(values["id"]);
        var name = values["name"];
        var ipAddress = values["ipAddress"];

        DeviceCreator creator = type.ToLower() switch
        {
            "camera" => new CameraCreator(id, name, ipAddress),
            "smartlight" => new SmartlightCreator(id, name, ipAddress, values.GetValueOrDefault("color", "White"), values.GetValueOrDefault("schedule", "Default")),
            "alarm" => new AlarmCreator(id, name, ipAddress),
            _ => throw new InvalidOperationException("Invalid device type")
        };

        return controladorIOT.addDevice(creator);
    }

    public bool removeDevice(int deviceId)
    {
        return controladorIOT.removeDevice(deviceId);
    }

    public void turnOnDevice(int deviceId)
    {
        var device = controladorIOT.findDeviceById(deviceId);
        if (device is ISwitchable switchable)
        {
            controladorIOT.executeCommand(new TurnOnCommand(switchable));
        }
    }

    public void turnOffDevice(int deviceId)
    {
        var device = controladorIOT.findDeviceById(deviceId);
        if (device is ISwitchable switchable)
        {
            controladorIOT.executeCommand(new TurnOffCommand(switchable));
        }
    }

    public void activateAlarm(int deviceId)
    {
        var device = controladorIOT.findDeviceById(deviceId);
        if (device is IAlarm alarm)
        {
            controladorIOT.executeCommand(new TriggerAlarmCommand(alarm));
        }
    }

    public void startRecording(int deviceId)
    {
        var device = controladorIOT.findDeviceById(deviceId);
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
        return controladorIOT.getAllDevices();
    }
}
