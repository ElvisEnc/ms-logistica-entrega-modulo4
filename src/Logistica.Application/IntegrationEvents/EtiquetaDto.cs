namespace Logistica.Application.IntegrationEvents;

// SISTEMA.md §9.11 — Etiqueta: objeto ESTRUCTURADO, no una cadena (RN-12)
public sealed record EtiquetaDto(
    Guid PaqueteId,
    string NombrePaciente,
    string NroIdentificacion,
    DireccionGeoDto DireccionEntrega,
    DateOnly Fecha,
    string? CodigoQR);
