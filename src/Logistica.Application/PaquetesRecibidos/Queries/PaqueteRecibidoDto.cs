using Logistica.Application.IntegrationEvents;

namespace Logistica.Application.PaquetesRecibidos.Queries;

public sealed record PaqueteRecibidoDto(
    Guid PaqueteId,
    Guid PacienteId,
    string PacienteNombre,
    DireccionGeoDto DireccionEntrega,
    Guid ContratoCateringId,
    EtiquetaDto Etiqueta,
    DateOnly FechaEntrega,
    EstadoAsignacion Estado);
