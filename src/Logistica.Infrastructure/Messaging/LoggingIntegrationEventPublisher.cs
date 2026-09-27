using System.Text.Json;
using System.Text.Json.Serialization;
using Logistica.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Logistica.Infrastructure.Messaging;

// §10.3 del maestro: unica implementacion de IIntegrationEventPublisher hoy. Es la unica prueba
// operativa de los tres eventos de integracion salientes de BC6.
public sealed class LoggingIntegrationEventPublisher(ILogger<LoggingIntegrationEventPublisher> logger) : IIntegrationEventPublisher
{
    // §12.11 del maestro: segundo de los DOS lugares independientes donde se registra
    // JsonStringEnumConverter (el primero es AddJsonOptions en Program.cs, Fase 5).
    // PropertyNamingPolicy = CamelCase es ADEMAS del converter: sin el, el log sale en
    // PascalCase y no coincide con el payload real de SISTEMA.md §9 — en BC6 este log es la
    // unica prueba de esos eventos. El converter NO lleva naming policy propia: los enums ya
    // estan en SCREAMING_SNAKE_CASE (§5.5, §12.11) y deben escribirse tal cual, sin camelCase.
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent, CancellationToken ct = default)
        where TIntegrationEvent : notnull
    {
        var payload = JsonSerializer.Serialize(integrationEvent, SerializerOptions);

        logger.LogInformation(
            "Evento de integracion publicado {TipoEvento}: {Payload}",
            typeof(TIntegrationEvent).Name,
            payload);

        return Task.CompletedTask;
    }
}
