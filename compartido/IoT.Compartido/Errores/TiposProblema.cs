namespace IoT.Compartido.Errores;

public static class TiposProblema
{
    private const string Base = "https://errors.umliot/";

    public const string NoAutenticado           = Base + "identidad/no-autenticado";
    public const string CredencialesInvalidas   = Base + "identidad/credenciales-invalidas";
    public const string CorreoYaRegistrado      = Base + "identidad/correo-ya-registrado";

    public const string DispositivoNoEncontrado = Base + "inventario/no-encontrado";
    public const string IpEnUso                 = Base + "inventario/ip-en-uso";

    public const string CapacidadNoSoportada    = Base + "control/capacidad-no-soportada";
    public const string TransicionInvalida      = Base + "estado/transicion-invalida";

    public const string Validacion              = Base + "comun/validacion";
}
