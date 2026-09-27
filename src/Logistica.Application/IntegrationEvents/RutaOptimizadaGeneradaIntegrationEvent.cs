namespace Logistica.Application.IntegrationEvents;

// SISTEMA.md §9.12 — BC6 -> consumidor externo. Se publica cada vez que Optimizar() termina
// con exito; una ruta re-optimizada emite un segundo evento (§9.1 DESIGN.md).
public sealed record RutaOptimizadaGeneradaIntegrationEvent(
    Guid RutaId,
    Guid RepartidorId,
    DateOnly Fecha,
    IReadOnlyCollection<ParadaOrdenadaDto> Paradas); // ordenadas por Orden
