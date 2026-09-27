using Joseco.DDD.Core.Results;
using Logistica.Domain.Repartidores;
using Logistica.Domain.Rutas.Events;
using MediatR;

namespace Logistica.Application.Rutas.Policies;

// §8.2 DESIGN.md: reacciona al evento de DOMINIO, nunca se invoca desde CompletarRutaCommandHandler.
// No llama a IUnitOfWork.CommitAsync: muta un Repartidor ya trackeado por el mismo DbContext, y su
// cambio entra en el SaveChanges que UnitOfWork.CommitAsync ejecuta justo despues de publicar (§12.6).
internal sealed class LiberarRepartidorAlCompletarRutaPolicy(IRepartidorRepository repartidores)
    : INotificationHandler<RutaCompletada>
{
    public async Task Handle(RutaCompletada notification, CancellationToken cancellationToken)
    {
        var repartidor = await repartidores.GetByIdAsync(notification.RepartidorId.Value)
            ?? throw new DomainException(RepartidorErrors.NoEncontrado()); // referencia interna rota: falla ruidosamente

        repartidor.Liberar(); // I11
    }
}
