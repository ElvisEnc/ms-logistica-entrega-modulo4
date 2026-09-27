using Logistica.Application.Repartidores.Queries;
using Logistica.Domain.Repartidores;
using Logistica.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistica.Infrastructure.Queries;

internal sealed class GetRepartidoresHandler(LogisticaDbContext context) : IRequestHandler<GetRepartidoresQuery, RepartidorDto[]>
{
    public async Task<RepartidorDto[]> Handle(GetRepartidoresQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Repartidor> query = context.Repartidores.AsNoTracking();

        if (request.SoloDisponibles == true)
            query = query.Where(r => r.Disponible);

        var repartidores = await query.ToListAsync(cancellationToken);

        return repartidores.Select(QueryDtoMapper.ToDto).ToArray();
    }
}
