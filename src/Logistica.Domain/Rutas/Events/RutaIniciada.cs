using Joseco.DDD.Core.Abstractions;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Rutas.Events;

public sealed record RutaIniciada(RutaId RutaId, RepartidorId RepartidorId) : DomainEvent;
