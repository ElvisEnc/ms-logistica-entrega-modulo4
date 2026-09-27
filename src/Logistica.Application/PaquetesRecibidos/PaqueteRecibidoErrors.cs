using Joseco.DDD.Core.Results;

namespace Logistica.Application.PaquetesRecibidos;

// Declarados aqui, en Application, y no en Domain: PaqueteRecibido no es un agregado (§9.4
// DESIGN.md) y estos codigos no los reutiliza ningun catalogo de Domain, a diferencia de
// I9/I10/I11/I2/EVIDENCIA_ARCHIVO_REQUERIDO/RANGO_FECHAS_INVALIDO/RUTA_NO_ENCONTRADA (§6.2, §13.3).
public static class PaqueteRecibidoErrors
{
    public static Error PacienteIdRequerido() =>
        new("PAQUETE_LISTO_PACIENTE_ID_REQUERIDO", "El pacienteId del paquete es obligatorio", ErrorType.Validation); // I14

    public static Error ContratoCateringRequerido() =>
        new("PAQUETE_LISTO_CONTRATO_CATERING_REQUERIDO", "El contratoCateringId del paquete es obligatorio", ErrorType.Validation); // I14
}
