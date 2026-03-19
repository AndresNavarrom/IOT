using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class StartRecordingCommand implements ICommand
{

    private IMonitorable device;

public StartRecordingCommand(IMonitorable device)
{
    this.device = device;
}

@Override
    public void execute()
{
    System.out.println("[Command] StartRecording");
    device.startRecording();
}
}
