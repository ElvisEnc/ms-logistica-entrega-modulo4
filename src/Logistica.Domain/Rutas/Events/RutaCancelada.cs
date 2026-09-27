using Joseco.DDD.Core.Abstractions;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Rutas.Events;

public sealed record RutaCancelada(
    RutaId RutaId,
    RepartidorId RepartidorId,
    string Motivo,
    IReadOnlyCollection<PaqueteId> PaqueteIdsNoResueltos) : DomainEvent;
