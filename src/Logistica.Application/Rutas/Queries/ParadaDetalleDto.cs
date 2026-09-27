using Logistica.Application.IntegrationEvents;
using Logistica.Domain.Rutas;

namespace Logistica.Application.Rutas.Queries;

public sealed record ParadaDetalleDto(
    Guid ParadaId,
    int Orden,
    Guid PaqueteId,
    Guid PacienteId,
    string PacienteNombre,
    DireccionGeoDto DireccionEntrega,
    EstadoEntrega Estado,
    ConstanciaEntregaDto? Constancia,
    IncidenciaEntregaDto? Incidencia);
