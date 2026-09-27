using Logistica.Application.Abstractions;
using Logistica.Domain.Rutas.Events;
using MediatR;

namespace Logistica.Application.Rutas.IntegrationEventHandlers;

// §8.3 DESIGN.md, SISTEMA.md §9.12
internal sealed class PublicarRutaOptimizadaGeneradaHandler(IIntegrationEventPublisher publisher)
    : INotificationHandler<RutaOptimizadaGenerada>
{
    public Task Handle(RutaOptimizadaGenerada notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(DomainToIntegrationMapper.ARutaOptimizadaGenerada(notification), cancellationToken);
}
