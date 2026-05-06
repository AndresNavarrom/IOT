namespace IoT.Compartido.Bus;

public interface IEventBus
{
    Task PublicarAsync<T>(string canal, T evento, CancellationToken ct = default) where T : class;

    void Suscribir<T>(string canal, Func<T, CancellationToken, Task> handler) where T : class;
}
