using MediatR;

namespace Logistica.Application.Rutas.Queries;

// HU-42. Devuelve lista vacia si no hay rutas ese dia: no es un 404 (§10.2 DESIGN.md).
public sealed record GetEstadoEntregasQuery(DateOnly Fecha) : IRequest<EstadoEntregaDto[]>;
