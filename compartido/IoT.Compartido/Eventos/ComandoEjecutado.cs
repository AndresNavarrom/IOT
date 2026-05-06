namespace IoT.Compartido.Eventos;

public sealed record ComandoEjecutado(
    Guid EventoId,
    Guid ComandoId,
    Guid DispositivoId,
    string Comando,
    Guid? EjecutadoPor,
    DateTimeOffset OcurridoEn);
