using Logistica.Domain.Repartidores;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Logistica.Infrastructure.Persistence.Repositories;

public sealed class RepartidorRepository(LogisticaDbContext context) : IRepartidorRepository
{
    public async Task<Repartidor?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        IQueryable<Repartidor> query = context.Repartidores;

        if (readOnly)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddAsync(Repartidor entity) => await context.Repartidores.AddAsync(entity);

    // I11 (§5.3, §12.5 del maestro): el id tipado COMPLETO en el Where, nunca .Value == guidSuelto.
    public Task<bool> TieneRutaActivaAsync(RepartidorId repartidorId) =>
        context.Rutas.AnyAsync(r =>
            r.RepartidorId == repartidorId &&
            (r.Estado == EstadoRuta.PENDIENTE || r.Estado == EstadoRuta.EN_CAMINO));
}
