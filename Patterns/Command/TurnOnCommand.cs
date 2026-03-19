using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class TurnOnCommand : ICommand
{

    private ISwitchable device;

public TurnOnCommand(ISwitchable device)
{
    this.device = device;
}

    public void execute()
{
    Console.WriteLine("[Command] TurnOn");
    device.turnOn();
}
}
