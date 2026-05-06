namespace IoT.Compartido.Eventos;

public sealed record EstadoCambiado(
    Guid EventoId,
    Guid DispositivoId,
    string Desde,
    string Hacia,
    string? Razon,
    DateTimeOffset OcurridoEn);
