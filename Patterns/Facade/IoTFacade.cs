using UMLIoT.Core.Controllers;
using UMLIoT.Core.Devices;
using UMLIoT.Core.Users;
using UMLIoT.Patterns.Command;
using UMLIoT.Patterns.Factory.Devices;
using DeviceCreator = UMLIoT.Patterns.Factory.Devices.DiviceCreator;

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

    // =========================
    // AUTENTICACIÓN
    // =========================

    public bool Login(string email, string password)
    {
        return authService.login(email, password, password);
    }

    public void Logout()
    {
        var user = userRepository.findById(userId);
        if (user is not null)
        {
            authService.logout();
        }
    }

    // =========================
    // DISPOSITIVOS
    // =========================

    public IDevice? RegisterDevice(string type, string config)
    {
        if (!creators.TryGetValue(type.ToLowerInvariant(), out var creator))
        {
            Console.WriteLine("Tipo de dispositivo no soportado");
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

    public bool RemoveDevice(int deviceId)
    {
        return controladorIOT.removeDevice(deviceId);
    }

    public List<IDevice> GetAllDevices()
    {
        var device = controladorIOT.findDeviceById(deviceId);
        if (device is ISwitchable switchable)
        {
            controladorIOT.executeCommand(new TurnOnCommand(switchable));
        }
    }

    public string GetDeviceStatus(int deviceId)
    {
        var device = controladorIOT.findDeviceById(deviceId);
        if (device is ISwitchable switchable)
        {
            controladorIOT.executeCommand(new TurnOffCommand(switchable));
        }
    }

    // =========================
    // ACCIONES (COMMAND)
    // =========================

    public void TurnOnDevice(int deviceId)
    {
        var device = controladorIOT.findDeviceById(deviceId);
        if (device is IAlarm alarm)
        {
            controladorIOT.executeCommand(new TriggerAlarmCommand(alarm));
        }
    }

    public void TurnOffDevice(int deviceId)
    {
        var device = controladorIOT.findDeviceById(deviceId);
        if (device is IMonitorable monitorable)
        {
            controladorIOT.executeCommand(new StartRecordingCommand(monitorable));
        }
    }

    public void ActivateAlarm(int deviceId)
    {
        ExecuteIf<IAlarm>(
            deviceId,
            device => new TriggerAlarmCommand(device),
            "El dispositivo no es una alarma"
        );
    }

    public void StartRecording(int deviceId)
    {
        return controladorIOT.getAllDevices();
    }
}