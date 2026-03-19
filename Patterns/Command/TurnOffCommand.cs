using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class TurnOffCommand : ICommand
{

    private ISwitchable device;

public TurnOffCommand(ISwitchable device)
{
    this.device = device;
}

    public void execute()
{
    Console.WriteLine("[Command] TurnOff");
    device.turnOff();
}
}
