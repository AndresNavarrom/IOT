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

    private readonly Dictionary<string, DeviceCreator> creators;

    public IoTFacade()
    {
        authService = new AuthService();
        userRepository = new UserRepository();
        controladorIOT = new ControladorIOT();

        // Registro de factories (mejor que switch)
        creators = new Dictionary<string, DeviceCreator>()
        {
            { "camera", new CameraCreator() },
            { "smartlight", new SmartlightCreator() },
            { "alarm", new AlarmCreator() }
        };
    }

    // =========================
    // AUTENTICACIÓN
    // =========================

    public bool Login(string email, string password)
    {
        return authService.login(email, password);
    }

    public void Logout()
    {
        authService.logout();
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

        return controladorIOT.addDevice(creator);
    }

    public bool RemoveDevice(int deviceId)
    {
        return controladorIOT.removeDevice(deviceId);
    }

    public List<IDevice> GetAllDevices()
    {
        return controladorIOT.getAllDevices();
    }

    public string GetDeviceStatus(int deviceId)
    {
        return controladorIOT.getDeviceStatus(deviceId);
    }

    // =========================
    // ACCIONES (COMMAND)
    // =========================

    public void TurnOnDevice(int deviceId)
    {
        ExecuteIf<ISwitchable>(
            deviceId,
            device => new TurnOnCommand(device),
            "El dispositivo no soporta encendido"
        );
    }

    public void TurnOffDevice(int deviceId)
    {
        ExecuteIf<ISwitchable>(
            deviceId,
            device => new TurnOffCommand(device),
            "El dispositivo no soporta apagado"
        );
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
        ExecuteIf<IMonitorable>(
            deviceId,
            device => new StartRecordingCommand(device),
            "El dispositivo no soporta grabación"
        );
    }

    // =========================
    // MÉTODO GENÉRICO (CLAVE)
    // =========================

    private void ExecuteIf<T>(
        int deviceId,
        Func<T, ICommand> commandFactory,
        string errorMessage
    ) where T : class
    {
        var device = controladorIOT.getDeviceById(deviceId);

        if (device is T typedDevice)
        {
            var command = commandFactory(typedDevice);
            controladorIOT.executeCommand(command);
        }
        else
        {
            Console.WriteLine(errorMessage);
        }
    }
}