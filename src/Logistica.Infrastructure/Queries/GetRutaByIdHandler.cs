using Joseco.DDD.Core.Results;
using Logistica.Application.Rutas.Queries;
using Logistica.Domain.Rutas;
using Logistica.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistica.Infrastructure.Queries;

internal sealed class GetRutaByIdHandler(LogisticaDbContext context) : IRequestHandler<GetRutaByIdQuery, RutaDetalleDto>
{
    public async Task<RutaDetalleDto> Handle(GetRutaByIdQuery request, CancellationToken cancellationToken)
    {
        // Comparacion por PK sin convertir: RutaEntrega.Id es un Guid plano, distinto de
        // RepartidorId (que si tiene ValueConverter) — no cae en la trampa de IDs tipados (§5.3).
        var ruta = await context.Rutas
            .Include("_paradas")
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RutaId, cancellationToken)
            ?? throw new DomainException(RutaErrors.RutaNoEncontrada());

        return QueryDtoMapper.ToDetalleDto(ruta);
    }
}
