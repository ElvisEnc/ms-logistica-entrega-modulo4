using Logistica.Domain.Rutas;

namespace Logistica.Application.Tests.TestSupport;

// Fake en memoria de IRutaEntregaRepository (UT-12, MAPA §6): IRutaEntregaRepository no agrega
// ningun metodo propio a IRepository<T> (§14.4 DESIGN.md, I9 se resuelve contra el store de
// paquetes recibidos, no contra las rutas), asi que este fake solo implementa GetByIdAsync y
// AddAsync.
public sealed class RutaEntregaRepositoryFake : IRutaEntregaRepository
{
    private readonly Dictionary<Guid, RutaEntrega> _rutas = new();

    // Para las aserciones "sin efectos" de UT-06 y para el camino feliz.
    public IReadOnlyCollection<RutaEntrega> RutasAgregadas => _rutas.Values.ToList();

    public Task<RutaEntrega?> GetByIdAsync(Guid id, bool readOnly = false) =>
        Task.FromResult(_rutas.GetValueOrDefault(id));

    public Task AddAsync(RutaEntrega entity)
    {
        _rutas[entity.Id] = entity;
        return Task.CompletedTask;
    }
}
