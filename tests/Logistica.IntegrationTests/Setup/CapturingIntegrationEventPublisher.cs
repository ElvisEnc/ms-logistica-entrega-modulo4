using Logistica.Application.Abstractions;

namespace Logistica.IntegrationTests.Setup;

// IT-04: sustituye a LoggingIntegrationEventPublisher en la WebApplicationFactory
// para capturar los eventos publicados y afirmarlos en los tests.
public sealed class CapturingIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly List<object> _events = [];

    public IReadOnlyList<object> Events => _events.AsReadOnly();

    public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken ct = default)
        where TIntegrationEvent : notnull
    {
        _events.Add(integrationEvent);
        return Task.CompletedTask;
    }

    public IReadOnlyList<TEvent> GetEvents<TEvent>() =>
        _events.OfType<TEvent>().ToList();

    public void Clear() => _events.Clear();
}
