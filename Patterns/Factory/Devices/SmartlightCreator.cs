using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Factory.Devices;

public class SmartlightCreator : DiviceCreator
{
    public override IDevice DeviceCreatorMethod()
    {
        return new Smartlight();
    }
}
