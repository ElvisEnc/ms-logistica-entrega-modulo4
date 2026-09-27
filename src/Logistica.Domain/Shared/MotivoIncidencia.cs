namespace Logistica.Domain.Shared;

// Miembros en SCREAMING_SNAKE_CASE (§5.5): el binding multipart no pasa por JsonStringEnumConverter.
public enum MotivoIncidencia
{
    PACIENTE_AUSENTE,
    DIRECCION_NO_ENCONTRADA,
    PAQUETE_DANADO,
    RECHAZADO_POR_PACIENTE,
    OTRO
}
