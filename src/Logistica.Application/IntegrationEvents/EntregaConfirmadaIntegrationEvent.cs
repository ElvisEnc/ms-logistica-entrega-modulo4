namespace Logistica.Application.IntegrationEvents;

// SISTEMA.md §9.13 — BC6 -> BC1. Nunca se publica una entrega sin evidencia (§9.1 DESIGN.md).
public sealed record EntregaConfirmadaIntegrationEvent(
    Guid RutaId,
    Guid ParadaId, // clave de idempotencia (§9.15 del maestro)
    Guid PaqueteId,
    Guid PacienteId, // heredado, sin transformar (I14)
    string PacienteNombre,
    ConstanciaDto Constancia); // ANIDADA, nunca aplanada
