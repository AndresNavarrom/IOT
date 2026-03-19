namespace UMLIoT.Core.Devices;

public interface IMonitorable
{
    string startRecording();
    string captureSnapshot();
}
