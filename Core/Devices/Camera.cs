namespace UMLIoT.Core.Devices;

public class Camera : Device, IMonitorable
{
    public Camera(int id, string name, string ipAddress) : base(id, name, ipAddress)
    {
    }

    public string startRecording()
    {
    connect();
    return $"Camera {getName()} recording";
    }

    public string captureSnapshot()
    {
    return $"Camera {getName()} snapshot captured";
    }
}
