using Logistica.Application.Abstractions;

namespace Logistica.Application.Tests.TestSupport;

// Fake de IIntegrationEventPublisher (UT-12, MAPA §6): captura el ultimo evento de integracion
// publicado para las aserciones de payload completo de §15.3 (UT-08, UT-09).
public sealed class IntegrationEventPublisherFake : IIntegrationEventPublisher
{
    public object? Publicado { get; private set; }

    public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken ct = default)
        where TIntegrationEvent : notnull
    {
        Publicado = integrationEvent;
        return Task.CompletedTask;
    }
}
