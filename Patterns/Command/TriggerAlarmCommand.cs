using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class TriggerAlarmCommand implements ICommand
{

    private IAlarm alarm;

public TriggerAlarmCommand(IAlarm alarm)
{
    this.alarm = alarm;
}

@Override
    public void execute()
{
    System.out.println("[Command] TriggerAlarm");
    alarm.trigger();
}
}
