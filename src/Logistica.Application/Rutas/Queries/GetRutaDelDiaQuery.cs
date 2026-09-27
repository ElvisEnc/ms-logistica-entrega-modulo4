using MediatR;

namespace Logistica.Application.Rutas.Queries;

// HU-39. RepartidorId se tipa ANTES de entrar al Where del handler (§5.3, §12.5 del maestro).
public sealed record GetRutaDelDiaQuery(Guid RepartidorId, DateOnly Fecha) : IRequest<RutaDetalleDto>;
