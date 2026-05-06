namespace IoT.Compartido.Eventos;

public sealed record DispositivoEliminado(
    Guid EventoId,
    Guid DispositivoId,
    DateTimeOffset OcurridoEn);
