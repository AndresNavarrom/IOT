using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class StartRecordingCommand : ICommand
{
    private readonly IMonitorable device;

    public StartRecordingCommand(IMonitorable device)
    {
        this.device = device;
    }

    public void execute()
    {
        device.startRecording();
    }
}
