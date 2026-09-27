using Logistica.Application.Abstractions;
using Logistica.Domain.Rutas.Events;
using MediatR;

namespace Logistica.Application.Rutas.IntegrationEventHandlers;

// §8.3 DESIGN.md, SISTEMA.md §9.14. I14 (RN-16, RN-23): traduce sin resolver el pacienteId ni el contratoCateringId, delegado a DomainToIntegrationMapper
internal sealed class PublicarIncidenciaEntregaRegistradaHandler(IIntegrationEventPublisher publisher)
    : INotificationHandler<IncidenciaEntregaRegistrada>
{
    public Task Handle(IncidenciaEntregaRegistrada notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(DomainToIntegrationMapper.AIncidenciaEntregaRegistrada(notification), cancellationToken);
}
