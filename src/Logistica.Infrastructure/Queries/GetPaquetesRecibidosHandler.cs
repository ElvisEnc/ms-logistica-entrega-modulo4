using Logistica.Application.Abstractions;
using Logistica.Application.PaquetesRecibidos.Queries;
using MediatR;

namespace Logistica.Infrastructure.Queries;

// Delega en IPaqueteRecibidoStore.ConsultarAsync, declarado exactamente para esto (§9.4
// DESIGN.md): no repite LINQ propio contra la tabla paquetes_recibidos.
internal sealed class GetPaquetesRecibidosHandler(IPaqueteRecibidoStore store) : IRequestHandler<GetPaquetesRecibidosQuery, PaqueteRecibidoDto[]>
{
    public async Task<PaqueteRecibidoDto[]> Handle(GetPaquetesRecibidosQuery request, CancellationToken cancellationToken)
    {
        var paquetes = await store.ConsultarAsync(request.Fecha, request.Estado, cancellationToken);

        return paquetes.Select(QueryDtoMapper.ToDto).ToArray();
    }
}
