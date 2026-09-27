namespace Logistica.Application.IntegrationEvents;

// SISTEMA.md §9.12 — ParadaOrdenada
public sealed record ParadaOrdenadaDto(
    Guid ParadaId,
    int Orden,
    Guid PaqueteId,
    Guid PacienteId, // heredado de BC5, SIN transformar
    string PacienteNombre,
    DireccionGeoDto DireccionEntrega);
