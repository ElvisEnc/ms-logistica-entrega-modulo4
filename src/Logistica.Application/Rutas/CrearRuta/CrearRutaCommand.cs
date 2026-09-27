using MediatR;

namespace Logistica.Application.Rutas.CrearRuta;

internal sealed record CrearRutaCommand(
    DateOnly Fecha,
    Guid RepartidorId,
    IReadOnlyCollection<Guid> PaqueteIds) : IRequest<Guid>;
