namespace UMLIoT.Core.Devices;

public class Camera : Device, IMonitorable
{
    public string startRecording()
    {
        return "Recording started";
    }

    public string captureSnapshot()
    {
        return "Snapshot captured";
    }
}
