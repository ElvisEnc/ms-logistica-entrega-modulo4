using MediatR;

namespace Logistica.Application.Rutas.Queries;

public sealed record GetRutaByIdQuery(Guid RutaId) : IRequest<RutaDetalleDto>;
