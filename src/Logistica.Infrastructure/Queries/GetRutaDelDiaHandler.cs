using Joseco.DDD.Core.Results;
using Logistica.Application.Rutas.Queries;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;
using Logistica.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistica.Infrastructure.Queries;

internal sealed class GetRutaDelDiaHandler(LogisticaDbContext context) : IRequestHandler<GetRutaDelDiaQuery, RutaDetalleDto>
{
    public async Task<RutaDetalleDto> Handle(GetRutaDelDiaQuery request, CancellationToken cancellationToken)
    {
        // §5.3, §12.5 del maestro: se tipa el guid ANTES de entrar al Where. Comparar
        // r.RepartidorId.Value == request.RepartidorId lanzaria InvalidOperationException en
        // tiempo de ejecucion contra PostgreSQL real.
        var repartidorId = RepartidorId.From(request.RepartidorId);

        var ruta = await context.Rutas
            .Include("_paradas")
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RepartidorId == repartidorId && r.Fecha == request.Fecha, cancellationToken)
            ?? throw new DomainException(RutaErrors.RutaNoEncontrada());

        return QueryDtoMapper.ToDetalleDto(ruta);
    }
}
