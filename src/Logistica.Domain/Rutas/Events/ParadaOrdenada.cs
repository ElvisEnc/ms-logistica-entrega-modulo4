using Logistica.Domain.Shared;

namespace Logistica.Domain.Rutas.Events;

public sealed record ParadaOrdenada(
    ParadaId ParadaId,
    int Orden,
    PaqueteId PaqueteId,
    PacienteId PacienteId,
    string PacienteNombre,
    DireccionGeo DireccionEntrega);
