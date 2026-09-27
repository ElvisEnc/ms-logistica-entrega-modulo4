using Logistica.Application.Rutas.Queries;
using Logistica.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistica.Infrastructure.Queries;

internal sealed class GetEstadoEntregasHandler(LogisticaDbContext context) : IRequestHandler<GetEstadoEntregasQuery, EstadoEntregaDto[]>
{
    public async Task<EstadoEntregaDto[]> Handle(GetEstadoEntregasQuery request, CancellationToken cancellationToken)
    {
        var rutas = await context.Rutas
            .Include("_paradas")
            .AsNoTracking()
            .Where(r => r.Fecha == request.Fecha)
            .ToListAsync(cancellationToken);

        // Lista vacia si no hay rutas ese dia: NO es un 404 (§10.2 DESIGN.md).
        return rutas
            .SelectMany(ruta => ruta.Paradas.Select(parada => QueryDtoMapper.ToEstadoEntregaDto(ruta, parada)))
            .ToArray();
    }
}
