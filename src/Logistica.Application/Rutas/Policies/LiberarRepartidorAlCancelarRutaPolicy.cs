using Joseco.DDD.Core.Results;
using Logistica.Domain.Repartidores;
using Logistica.Domain.Rutas.Events;
using MediatR;

namespace Logistica.Application.Rutas.Policies;

// §8.2 DESIGN.md: simetrica a LiberarRepartidorAlCompletarRutaPolicy, reaccionando a RutaCancelada.
internal sealed class LiberarRepartidorAlCancelarRutaPolicy(IRepartidorRepository repartidores)
    : INotificationHandler<RutaCancelada>
{
    public async Task Handle(RutaCancelada notification, CancellationToken cancellationToken)
    {
        var repartidor = await repartidores.GetByIdAsync(notification.RepartidorId.Value)
            ?? throw new DomainException(RepartidorErrors.NoEncontrado()); // referencia interna rota: falla ruidosamente

        repartidor.Liberar(); // I11
    }
}
