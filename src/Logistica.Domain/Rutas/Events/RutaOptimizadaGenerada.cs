using Joseco.DDD.Core.Abstractions;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Rutas.Events;

public sealed record RutaOptimizadaGenerada(
    RutaId RutaId,
    RepartidorId RepartidorId,
    DateOnly Fecha,
    IReadOnlyCollection<ParadaOrdenada> ParadasOrdenadas) : DomainEvent;
