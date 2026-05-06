using System.Collections.Concurrent;

namespace IoT.Compartido.Bus;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<Func<object, CancellationToken, Task>>> handlers = new();

    public async Task PublicarAsync<T>(string canal, T evento, CancellationToken ct = default) where T : class
    {
        if (!handlers.TryGetValue(canal, out var bag))
        {
            return;
        }

        foreach (var handler in bag)
        {
            await handler(evento, ct);
        }
    }

    public void Suscribir<T>(string canal, Func<T, CancellationToken, Task> handler) where T : class
    {
        var bag = handlers.GetOrAdd(canal, _ => new ConcurrentBag<Func<object, CancellationToken, Task>>());
        bag.Add((obj, ct) => handler((T)obj, ct));
    }
}
