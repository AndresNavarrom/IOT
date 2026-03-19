using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Command;

public class TurnOffCommand : ICommand
{
    private readonly ISwitchable device;

    public TurnOffCommand(ISwitchable device)
    {
        this.device = device;
    }

    public void execute()
    {
        device.turnOff();
    }
}
