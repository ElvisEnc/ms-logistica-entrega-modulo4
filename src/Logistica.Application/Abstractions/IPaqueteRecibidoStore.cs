using Logistica.Application.PaquetesRecibidos;

namespace Logistica.Application.Abstractions;

// §9.4 DESIGN.md: puerto del read model. NO hereda de IRepository<T> — PaqueteRecibido no es
// un agregado y no debe presentarse como tal.
public interface IPaqueteRecibidoStore
{
    // I12: si el paqueteId ya existe, no hace nada — ni inserta ni actualiza (Infrastructure, fase 4)
    public Task UpsertAsync(PaqueteRecibido paquete, CancellationToken ct = default);

    // Todos los POR_ASIGNAR SIN filtrar por fecha, a proposito: con ese pool completo,
    // CrearRutaCommandHandler distingue I9 de I10 en una sola consulta (§6.3)
    public Task<IReadOnlyCollection<PaqueteRecibido>> ObtenerPorAsignarAsync(CancellationToken ct = default);

    public Task<IReadOnlyCollection<PaqueteRecibido>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default);

    // I9: al crear la ruta
    public Task MarcarAsignadosAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default);

    // I9: al cancelarla, desde ReponerPaquetesAlCancelarRutaPolicy
    public Task MarcarPorAsignarAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default);

    // Alimenta GetPaquetesRecibidosQuery
    public Task<IReadOnlyCollection<PaqueteRecibido>> ConsultarAsync(DateOnly fecha, EstadoAsignacion? estado, CancellationToken ct = default);
}
