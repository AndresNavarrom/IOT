using UMLIoT.Core.Devices;

namespace UMLIoT.Patterns.Factory.Devices;

public class CameraCreator : DiviceCreator
{
    public override IDevice DeviceCreatorMethod()
    {
        return new Camera();
    }
}
