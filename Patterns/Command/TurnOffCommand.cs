using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class TurnOffCommand implements ICommand
{

    private ISwitchable device;

public TurnOffCommand(ISwitchable device)
{
    this.device = device;
}

@Override
    public void execute()
{
    System.out.println("[Command] TurnOff");
    device.turnOff();
}
}
