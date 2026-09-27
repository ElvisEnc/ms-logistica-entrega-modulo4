using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Rutas;

public static class PaqueteParaRutaErrors
{
    public static Error PaqueteIdRequerido() =>
        new("PAQUETE_RUTA_PAQUETE_ID_REQUERIDO", "El id del paquete es obligatorio", ErrorType.Validation);

    public static Error PacienteIdRequerido() =>
        new("PAQUETE_RUTA_PACIENTE_ID_REQUERIDO", "El id del paciente es obligatorio", ErrorType.Validation); // I14

    public static Error PacienteNombreRequerido() =>
        new("PAQUETE_RUTA_PACIENTE_NOMBRE_REQUERIDO", "El nombre del paciente es obligatorio", ErrorType.Validation);

    public static Error DireccionRequerida() =>
        new("PAQUETE_RUTA_DIRECCION_REQUERIDA", "La direccion de entrega es obligatoria", ErrorType.Validation);

    public static Error ContratoCateringRequerido() =>
        new("PAQUETE_RUTA_CONTRATO_CATERING_REQUERIDO", "El id del contrato de catering es obligatorio", ErrorType.Validation); // I14
}
