using UMLIoT.Patterns.State;

namespace UMLIoT.Core.Devices;

public class Device : IDevice
{
    private int id;
    private string name;
    private DeviceStatus status;
    private string ipAddress;

    protected Device(int id, string name, string ipAddress)
    {
    this.id = id;
    this.name = name;
    this.ipAddress = ipAddress;
    status = new OfflineStatus();
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
    status.handleStatus();
    }

    public virtual int getId()
    {
    return id;
    }

    public virtual int getID()
    {
    return getId();
    }

    public virtual string getName()
    {
    return name;
    }

}
