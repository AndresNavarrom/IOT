using UMLIoT.Patterns.State;

namespace UMLIoT.Core.Devices;

public class Device : IDevice
{
    private int id;
    private string name;
    private DeviceStatus status;
    private string ipAddress;

    public Device()
    {
        id = 0;
        name = string.Empty;
        status = new OfflineStatus();
        ipAddress = string.Empty;
    }

    public Device(int id, string name, DeviceStatus status, string ipAddress)
    {
        this.id = id;
        this.name = name;
        this.status = status;
        this.ipAddress = ipAddress;
    }

    public virtual bool connect()
    {
        status = new OnlineStatus();
        return true;
    }

    public virtual bool disconnect()
    {
        status = new OfflineStatus();
        return true;
    }

    public virtual DeviceStatus getStatus()
    {
        return status;
    }

    public virtual void handleStatus()
    {
        status.handleStatus(this);
    }

    public virtual int getId()
    {
        return id;
    }
}
