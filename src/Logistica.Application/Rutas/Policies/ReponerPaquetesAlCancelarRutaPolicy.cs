using Logistica.Application.Abstractions;
using Logistica.Domain.Rutas.Events;
using MediatR;

namespace Logistica.Application.Rutas.Policies;

// §8.2 DESIGN.md: lee los PaqueteId DEL EVENTO, no vuelve a cargar el agregado. Escribe sobre
// IPaqueteRecibidoStore, que NO es un agregado y persiste sus propios cambios de inmediato con
// su propio SaveChangesAsync: no participa de la atomicidad del UnitOfWork (§8.2, §12.6 del
// maestro). Es idempotente: marcar POR_ASIGNAR un paquete ya POR_ASIGNAR no hace daño.
internal sealed class ReponerPaquetesAlCancelarRutaPolicy(IPaqueteRecibidoStore paquetesRecibidos)
    : INotificationHandler<RutaCancelada>
{
    public async Task Handle(RutaCancelada notification, CancellationToken cancellationToken)
    {
        if (notification.PaqueteIdsNoResueltos.Count == 0)
            return;

        var paqueteIds = notification.PaqueteIdsNoResueltos.Select(id => id.Value).ToList();

        await paquetesRecibidos.MarcarPorAsignarAsync(paqueteIds, cancellationToken); // I9 (RN-17)
    }
}
