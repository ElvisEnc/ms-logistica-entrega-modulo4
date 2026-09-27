using MediatR;

namespace Logistica.Application.PaquetesRecibidos.Queries;

public sealed record GetPaquetesRecibidosQuery(DateOnly Fecha, EstadoAsignacion? Estado) : IRequest<PaqueteRecibidoDto[]>;
