namespace Logistica.Application.Abstractions;

// §10.3 del maestro: puerto de salida de Application. Su unica implementacion hoy es
// LoggingIntegrationEventPublisher (Infrastructure, fase 4), que serializa a JSON y lo registra en el log.
public interface IIntegrationEventPublisher
{
    public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken ct = default)
        where TIntegrationEvent : notnull;
}
