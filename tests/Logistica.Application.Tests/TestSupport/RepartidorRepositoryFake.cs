using Logistica.Domain.Repartidores;
using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.TestSupport;

// Fake en memoria de IRepartidorRepository (UT-12, MAPA §6). TieneRutaActivaAsync se resuelve
// por configuracion explicita del test (la propiedad TieneRutaActiva), NUNCA consultando una
// lista real de rutas: en produccion I11 es una consulta sobre la tabla rutas completa
// (§6.2 DESIGN.md) que este nivel de test no reproduce ni debe reproducir.
public sealed class RepartidorRepositoryFake : IRepartidorRepository
{
    private readonly Dictionary<Guid, Repartidor> _repartidores = new();

    public bool TieneRutaActiva { get; set; }

    public void Agregar(Repartidor repartidor) => _repartidores[repartidor.Id] = repartidor;

    public Task<Repartidor?> GetByIdAsync(Guid id, bool readOnly = false) =>
        Task.FromResult(_repartidores.GetValueOrDefault(id));

    public Task AddAsync(Repartidor entity)
    {
        _repartidores[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task<bool> TieneRutaActivaAsync(RepartidorId repartidorId) => Task.FromResult(TieneRutaActiva);
}
