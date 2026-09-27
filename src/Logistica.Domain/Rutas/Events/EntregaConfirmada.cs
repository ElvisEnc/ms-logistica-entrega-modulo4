using Joseco.DDD.Core.Abstractions;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Rutas.Events;

public sealed record EntregaConfirmada(
    RutaId RutaId,
    ParadaId ParadaId,
    PaqueteId PaqueteId,
    PacienteId PacienteId,
    string PacienteNombre,
    ConstanciaEntrega Constancia) : DomainEvent;
