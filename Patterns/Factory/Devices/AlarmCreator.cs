using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Factory.Devices;

public class AlarmCreator : DiviceCreator
{
    public override IDevice DeviceCreatorMethod()
    {
        return new Alarm();
    }
}
