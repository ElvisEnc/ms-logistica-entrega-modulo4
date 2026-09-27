using MediatR;

namespace Logistica.Application.Repartidores.Queries;

// El IRequestHandler vive en Infrastructure/Queries/ (§10.2, §12.8 del maestro): necesita
// LINQ y EF Core directos, que Application no puede referenciar.
public sealed record GetRepartidoresQuery(bool? SoloDisponibles) : IRequest<RepartidorDto[]>;
