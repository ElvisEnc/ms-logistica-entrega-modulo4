namespace Logistica.Application.IntegrationEvents;

// SISTEMA.md §9.11 — PaqueteListo
public sealed record PaqueteListoDto(
    Guid PaqueteId, // identificador de tracking (RN-17)
    Guid PacienteId,
    string PacienteNombre,
    DireccionGeoDto DireccionEntrega,
    Guid ContratoCateringId, // BC6 lo devuelve a BC4 en la incidencia
    EtiquetaDto Etiqueta);
