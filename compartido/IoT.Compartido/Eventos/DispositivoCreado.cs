namespace IoT.Compartido.Eventos;

public sealed record DispositivoCreado(
    Guid EventoId,
    Guid DispositivoId,
    string Tipo,
    string Nombre,
    Guid PropietarioId,
    string IpAddress,
    DateTimeOffset OcurridoEn);
