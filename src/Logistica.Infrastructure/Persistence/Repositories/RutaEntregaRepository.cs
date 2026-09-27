using Logistica.Domain.Rutas;
using Microsoft.EntityFrameworkCore;

namespace Logistica.Infrastructure.Persistence.Repositories;

public sealed class RutaEntregaRepository(LogisticaDbContext context) : IRutaEntregaRepository
{
    public async Task<RutaEntrega?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        // Include("_paradas") POR STRING: el backing field es privado (§14.4 DESIGN.md).
        IQueryable<RutaEntrega> query = context.Rutas.Include("_paradas");

        if (readOnly)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddAsync(RutaEntrega entity) => await context.Rutas.AddAsync(entity);
}
