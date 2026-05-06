namespace IoT.Compartido.Bus;

public static class CanalesEventos
{
    public const string DispositivoCreado     = "dispositivo.ciclo-vida.creado";
    public const string DispositivoEliminado  = "dispositivo.ciclo-vida.eliminado";
    public const string Encendido             = "dispositivo.control.encendido";
    public const string Apagado               = "dispositivo.control.apagado";
    public const string AlarmaDisparada       = "dispositivo.control.alarma-disparada";
    public const string GrabacionIniciada     = "dispositivo.control.grabacion-iniciada";
    public const string EstadoCambiado        = "dispositivo.estado.cambiado";
}
