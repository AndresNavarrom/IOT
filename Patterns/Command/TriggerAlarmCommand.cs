using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class TriggerAlarmCommand : ICommand
{

    private IAlarm alarm;

public TriggerAlarmCommand(IAlarm alarm)
{
    this.alarm = alarm;
}

    public void execute()
{
    Console.WriteLine("[Command] TriggerAlarm");
    alarm.trigger();
}
}
