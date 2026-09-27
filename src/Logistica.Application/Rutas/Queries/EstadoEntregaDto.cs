using Logistica.Domain.Rutas;

namespace Logistica.Application.Rutas.Queries;

public sealed record EstadoEntregaDto(
    Guid RutaId,
    Guid RepartidorId,
    Guid ParadaId,
    Guid PaqueteId,
    string PacienteNombre,
    EstadoEntrega Estado);
