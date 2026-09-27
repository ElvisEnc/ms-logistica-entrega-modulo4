using Logistica.Domain.Rutas;

namespace Logistica.Application.Rutas.Queries;

public sealed record RutaDetalleDto(
    Guid RutaId,
    Guid RepartidorId,
    DateOnly Fecha,
    EstadoRuta Estado,
    bool Optimizada, // derivada en el mapeo: hay paradas y todas tienen Orden >= 1 (§10.2)
    decimal DistanciaTotalKm,
    int TiempoEstimadoMin,
    IReadOnlyCollection<ParadaDetalleDto> Paradas);
