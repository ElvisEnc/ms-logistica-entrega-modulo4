using Logistica.Application.Abstractions;
using Logistica.Domain.Rutas.Events;
using MediatR;

namespace Logistica.Application.Rutas.IntegrationEventHandlers;

// §8.3 DESIGN.md, SISTEMA.md §9.13. I14 (RN-16, RN-23): traduce sin resolver el pacienteId, delegado a DomainToIntegrationMapper
internal sealed class PublicarEntregaConfirmadaHandler(IIntegrationEventPublisher publisher)
    : INotificationHandler<EntregaConfirmada>
{
    public Task Handle(EntregaConfirmada notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(DomainToIntegrationMapper.AEntregaConfirmada(notification), cancellationToken);
}
