using Logistica.Domain.Shared;

namespace Logistica.Application.IntegrationEvents;

// SISTEMA.md §9.14 — BC6 -> BC1 y BC4. DIEZ campos, no nueve: urlFoto existe siempre en el
// DTO de ambos consumidores aunque su valor sea null.
public sealed record IncidenciaEntregaRegistradaIntegrationEvent(
    Guid RutaId,
    Guid ParadaId, // clave de idempotencia del conteo de BC4
    Guid PaqueteId,
    Guid PacienteId, // heredado del paquete, nunca resuelto (I14)
    string PacienteNombre,
    Guid ContratoCateringId, // heredado del paquete, nunca resuelto (I14)
    MotivoIncidencia Motivo, // sin interpretar
    string Descripcion,
    DateTime FechaHora,
    string? UrlFoto);
