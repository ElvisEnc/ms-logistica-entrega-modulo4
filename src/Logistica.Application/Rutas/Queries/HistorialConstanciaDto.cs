using Logistica.Domain.Rutas;

namespace Logistica.Application.Rutas.Queries;

public sealed record HistorialConstanciaDto(
    Guid RutaId,
    Guid ParadaId,
    Guid PaqueteId,
    Guid PacienteId,
    string PacienteNombre,
    DateOnly Fecha,
    EstadoEntrega Estado,
    ConstanciaEntregaDto? Constancia,
    IncidenciaEntregaDto? Incidencia);
