using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class TurnOnCommand : ICommand
{
    private readonly ISwitchable device;

    public TurnOnCommand(ISwitchable device)
    {
        this.device = device;
    }

    public void execute()
    {
        device.turnOn();
    }
}
