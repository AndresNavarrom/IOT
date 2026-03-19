using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class TriggerAlarmCommand : ICommand
{
    private readonly IAlarm device;

    public TriggerAlarmCommand(IAlarm device)
    {
        this.device = device;
    }

    public void execute()
    {
        device.trigger();
    }
}
