using FluentAssertions;
using IoT.Compartido.Bus;
using IoT.Compartido.Eventos;

namespace IoT.Compartido.Pruebas;

public class InMemoryEventBusTests
{
    private static DispositivoCreado UnEventoCreado() =>
        new(
            EventoId: Guid.NewGuid(),
            DispositivoId: Guid.NewGuid(),
            Tipo: "camara",
            Nombre: "cam-1",
            PropietarioId: Guid.NewGuid(),
            IpAddress: "192.168.1.5",
            OcurridoEn: DateTimeOffset.UtcNow);

    [Fact]
    public async Task Publicar_sin_suscriptores_no_lanza_excepcion()
    {
        var bus = new InMemoryEventBus();

        var act = () => bus.PublicarAsync(CanalesEventos.DispositivoCreado, UnEventoCreado());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Publicar_invoca_a_los_suscriptores_del_canal()
    {
        var bus = new InMemoryEventBus();
        DispositivoCreado? recibido = null;

        bus.Suscribir<DispositivoCreado>(CanalesEventos.DispositivoCreado, (e, _) =>
        {
            recibido = e;
            return Task.CompletedTask;
        });

        var evento = UnEventoCreado();
        await bus.PublicarAsync(CanalesEventos.DispositivoCreado, evento);

        recibido.Should().Be(evento);
    }

    [Fact]
    public async Task Publicar_solo_invoca_handlers_del_canal_correcto()
    {
        var bus = new InMemoryEventBus();
        var contadorCreado = 0;
        var contadorEliminado = 0;

        bus.Suscribir<DispositivoCreado>(CanalesEventos.DispositivoCreado,
            (_, _) => { contadorCreado++; return Task.CompletedTask; });
        bus.Suscribir<DispositivoEliminado>(CanalesEventos.DispositivoEliminado,
            (_, _) => { contadorEliminado++; return Task.CompletedTask; });

        await bus.PublicarAsync(CanalesEventos.DispositivoCreado, UnEventoCreado());

        contadorCreado.Should().Be(1);
        contadorEliminado.Should().Be(0);
    }

    [Fact]
    public async Task Publicar_invoca_a_todos_los_handlers_del_canal()
    {
        var bus = new InMemoryEventBus();
        var invocaciones = 0;

        for (var i = 0; i < 3; i++)
        {
            bus.Suscribir<DispositivoCreado>(CanalesEventos.DispositivoCreado,
                (_, _) => { Interlocked.Increment(ref invocaciones); return Task.CompletedTask; });
        }

        await bus.PublicarAsync(CanalesEventos.DispositivoCreado, UnEventoCreado());

        invocaciones.Should().Be(3);
    }
}
