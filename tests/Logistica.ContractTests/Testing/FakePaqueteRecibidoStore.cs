using Logistica.Application.Abstractions;
using Logistica.Application.PaquetesRecibidos;

namespace Logistica.ContractTests.Testing;

// CT-04: sustituye el puerto de persistencia del lado consumidor (par #9) por un fake que
// captura en memoria. Lo que prueba el contract test es la deserializacion del mensaje y la
// construccion de los VO reales dentro del handler real, no el efecto de persistencia — eso
// ya lo cubre el nivel de integracion (IT-04).
public sealed class FakePaqueteRecibidoStore : IPaqueteRecibidoStore
{
    public List<PaqueteRecibido> Capturados { get; } = [];

    public Task UpsertAsync(PaqueteRecibido paquete, CancellationToken ct = default)
    {
        Capturados.Add(paquete);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<PaqueteRecibido>> ObtenerPorAsignarAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyCollection<PaqueteRecibido>>(Capturados);

    public Task<IReadOnlyCollection<PaqueteRecibido>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyCollection<PaqueteRecibido>>(Capturados.Where(p => paqueteIds.Contains(p.PaqueteId)).ToList());

    public Task MarcarAsignadosAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default) => Task.CompletedTask;

    public Task MarcarPorAsignarAsync(IReadOnlyCollection<Guid> paqueteIds, CancellationToken ct = default) => Task.CompletedTask;

    public Task<IReadOnlyCollection<PaqueteRecibido>> ConsultarAsync(DateOnly fecha, EstadoAsignacion? estado, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyCollection<PaqueteRecibido>>(Capturados);
}
