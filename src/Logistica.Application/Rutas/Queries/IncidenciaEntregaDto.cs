using Logistica.Domain.Shared;

namespace Logistica.Application.Rutas.Queries;

public sealed record IncidenciaEntregaDto(
    MotivoIncidencia Motivo,
    string Descripcion,
    DateTime FechaHora,
    string? UrlFoto);
