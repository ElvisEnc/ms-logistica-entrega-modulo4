using Joseco.DDD.Core.Abstractions;

namespace Logistica.Domain.Rutas;

// I9 se resuelve contra el store de paquetes recibidos, no contra las rutas: no se declara
// ningun metodo propio que nadie necesita (§14.4 del DESIGN.md).
public interface IRutaEntregaRepository : IRepository<RutaEntrega>
{
}
