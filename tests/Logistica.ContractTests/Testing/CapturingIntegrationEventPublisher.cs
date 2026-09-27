using System.Text.Json;
using System.Text.Json.Serialization;
using Logistica.Application.Abstractions;

namespace Logistica.ContractTests.Testing;

// CT-05: mismas JsonSerializerOptions que el publicador real
// (src/Logistica.Infrastructure/Messaging/LoggingIntegrationEventPublisher.cs), copiadas
// literalmente, no recreadas con otro valor.
public sealed class CapturingIntegrationEventPublisher : IIntegrationEventPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public string? UltimoPayloadJson { get; private set; }
    public string? UltimoTipoEvento { get; private set; }

    public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken ct = default)
        where TIntegrationEvent : notnull
    {
        UltimoTipoEvento = typeof(TIntegrationEvent).Name;
        UltimoPayloadJson = JsonSerializer.Serialize(integrationEvent, SerializerOptions);
        return Task.CompletedTask;
    }

    public void Limpiar()
    {
        UltimoPayloadJson = null;
        UltimoTipoEvento = null;
    }
}
