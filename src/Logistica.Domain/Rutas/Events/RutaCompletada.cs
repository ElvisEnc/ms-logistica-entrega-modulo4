using Joseco.DDD.Core.Abstractions;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Rutas.Events;

public sealed record RutaCompletada(RutaId RutaId, RepartidorId RepartidorId, EstadoRuta EstadoFinal) : DomainEvent;
