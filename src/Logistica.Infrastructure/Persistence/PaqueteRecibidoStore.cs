using Logistica.Application.Abstractions;
using Logistica.Application.PaquetesRecibidos;
using Microsoft.EntityFrameworkCore;

namespace Logistica.Infrastructure.Persistence;

// §14.4 DESIGN.md: excepcion consciente al patron de UnitOfWork. PaqueteRecibido no es un
// agregado, asi que este store no participa del UnitOfWork y persiste sus propios cambios de
// inmediato con su propio SaveChangesAsync.
public sealed class PaqueteRecibidoStore(LogisticaDbContext context) : IPaqueteRecibidoStore
{
    public async Task UpsertAsync(PaqueteRecibido paquete, CancellationToken ct = default)
    {
        // I12: si el paqueteId ya existe, NO HACE NADA — ni inserta ni actualiza. Sobrescribir
        // devolveria a POR_ASIGNAR un paquete ya dentro de una ruta y permitiria asignarlo dos
        // veces, violando I9 en silencio (§9.3 DESIGN.md).
        var existe = await context.PaquetesRecibidos.AnyAsync(p => p.PaqueteId == paquete.PaqueteId, ct);
        if (existe)
            return;

        await context.PaquetesRecibidos.AddAsync(paquete, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyCollection<PaqueteRecibido>> ObtenerPorAsignarAsync(CancellationToken ct = default) =>
        await context.PaquetesRecibidos
            .AsNoTracking()
            .Where(p => p.Estado == EstadoAsignacion.POR_ASIGNAR)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<PaqueteRecibido>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default) =>
        await context.PaquetesRecibidos
            .AsNoTracking()
            .Where(p => paqueteIds.Contains(p.PaqueteId))
            .ToListAsync(ct);

    public Task MarcarAsignadosAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default) =>
        CambiarEstadoAsync(paqueteIds, EstadoAsignacion.ASIGNADO, ct);

    public Task MarcarPorAsignarAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default) =>
        CambiarEstadoAsync(paqueteIds, EstadoAsignacion.POR_ASIGNAR, ct);

    public async Task<IReadOnlyCollection<PaqueteRecibido>> ConsultarAsync(DateOnly fecha, EstadoAsignacion? estado, CancellationToken ct = default) =>
        await context.PaquetesRecibidos
            .AsNoTracking()
            .Where(p => p.FechaEntrega == fecha && (estado == null || p.Estado == estado))
            .ToListAsync(ct);

    // El cambio de Estado se hace sobre el valor TRACKEADO, nunca reasignando la propiedad init
    // del record (§9.4 DESIGN.md: el read model es una proyeccion, no un objeto con comportamiento).
    private async Task CambiarEstadoAsync(IReadOnlyCollection<Guid> paqueteIds, EstadoAsignacion estado, CancellationToken ct)
    {
        if (paqueteIds.Count == 0)
            return;

        var filas = await context.PaquetesRecibidos
            .Where(p => paqueteIds.Contains(p.PaqueteId))
            .ToListAsync(ct);

        foreach (var fila in filas)
            context.Entry(fila).CurrentValues.SetValues(new { Estado = estado });

        await context.SaveChangesAsync(ct);
    }
}
