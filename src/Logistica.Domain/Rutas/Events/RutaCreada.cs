using Joseco.DDD.Core.Abstractions;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Rutas.Events;

public sealed record RutaCreada(
    RutaId RutaId,
    RepartidorId RepartidorId,
    DateOnly Fecha,
    IReadOnlyCollection<PaqueteId> PaqueteIds) : DomainEvent;
