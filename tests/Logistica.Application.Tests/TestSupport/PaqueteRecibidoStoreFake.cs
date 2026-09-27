using Logistica.Application.Abstractions;
using Logistica.Application.PaquetesRecibidos;

namespace Logistica.Application.Tests.TestSupport;

// Fake en memoria de IPaqueteRecibidoStore (UT-12, MAPA §6): el unico store con logica propia
// que probar es I12 — UpsertAsync ignora el duplicado, nunca lo sobrescribe (§9.3 DESIGN.md,
// PaqueteRecibidoStore.UpsertAsync real en Infrastructure) — asi que el fake reproduce esa
// semantica en vez de comportarse como un diccionario tonto.
public sealed class PaqueteRecibidoStoreFake : IPaqueteRecibidoStore
{
    private readonly Dictionary<Guid, PaqueteRecibido> _paquetes = new();

    public IReadOnlyCollection<PaqueteRecibido> Paquetes => _paquetes.Values.ToList();

    public void Agregar(PaqueteRecibido paquete) => _paquetes[paquete.PaqueteId] = paquete;

    public Task UpsertAsync(PaqueteRecibido paquete, CancellationToken ct = default)
    {
        // I12: si el paqueteId ya existe, no se sobrescribe (§9.3 DESIGN.md)
        _paquetes.TryAdd(paquete.PaqueteId, paquete);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<PaqueteRecibido>> ObtenerPorAsignarAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyCollection<PaqueteRecibido>>(
            _paquetes.Values.Where(p => p.Estado == EstadoAsignacion.POR_ASIGNAR).ToList());

    public Task<IReadOnlyCollection<PaqueteRecibido>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyCollection<PaqueteRecibido>>(
            _paquetes.Values.Where(p => paqueteIds.Contains(p.PaqueteId)).ToList());

    public Task MarcarAsignadosAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default) =>
        CambiarEstado(paqueteIds, EstadoAsignacion.ASIGNADO); // I9 (RN-17)

    public Task MarcarPorAsignarAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default) =>
        CambiarEstado(paqueteIds, EstadoAsignacion.POR_ASIGNAR); // I9 (RN-17), reposicion al cancelar

    public Task<IReadOnlyCollection<PaqueteRecibido>> ConsultarAsync(DateOnly fecha, EstadoAsignacion? estado, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyCollection<PaqueteRecibido>>(
            _paquetes.Values.Where(p => p.FechaEntrega == fecha && (estado == null || p.Estado == estado)).ToList());

    private Task CambiarEstado(IReadOnlyCollection<Guid> paqueteIds, EstadoAsignacion estado)
    {
        foreach (var id in paqueteIds)
            if (_paquetes.TryGetValue(id, out var paquete))
                _paquetes[id] = paquete with { Estado = estado };

        return Task.CompletedTask;
    }
}
