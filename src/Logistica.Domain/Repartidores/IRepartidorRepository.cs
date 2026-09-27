using Joseco.DDD.Core.Abstractions;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Repartidores;

public interface IRepartidorRepository : IRepository<Repartidor>
{
    // I11: consulta sobre la tabla rutas entera, respaldada ademas por indice unico filtrado (Infrastructure)
    public Task<bool> TieneRutaActivaAsync(RepartidorId repartidorId);
}
