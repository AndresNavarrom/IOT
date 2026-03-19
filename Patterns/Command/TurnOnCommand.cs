using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class TurnOnCommand implements ICommand
{

    private ISwitchable device;

public TurnOnCommand(ISwitchable device)
{
    this.device = device;
}

@Override
    public void execute()
{
    System.out.println("[Command] TurnOn");
    device.turnOn();
}
}
