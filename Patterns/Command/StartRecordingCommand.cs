using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class StartRecordingCommand : ICommand
{

    private IMonitorable device;

public StartRecordingCommand(IMonitorable device)
{
    this.device = device;
}

    public void execute()
{
    Console.WriteLine("[Command] StartRecording");
    device.startRecording();
}
}
