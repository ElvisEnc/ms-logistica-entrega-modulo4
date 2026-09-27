using Joseco.DDD.Core.Results;
using Logistica.Application.Rutas.Queries;
using Logistica.Domain.Rutas;
using Logistica.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistica.Infrastructure.Queries;

internal sealed class GetMapaRutaHandler(LogisticaDbContext context) : IRequestHandler<GetMapaRutaQuery, string>
{
    public async Task<string> Handle(GetMapaRutaQuery request, CancellationToken cancellationToken)
    {
        var ruta = await context.Rutas
            .Include("_paradas")
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RutaId, cancellationToken)
            ?? throw new DomainException(RutaErrors.RutaNoEncontrada());

        return MapaRutaHtmlBuilder.Construir(ruta);
    }
}
