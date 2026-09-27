using Logistica.Domain.Shared;

namespace Logistica.Application.IntegrationEvents;

// SISTEMA.md §9.13 — Constancia, ANIDADA dentro de EntregaConfirmadaIntegrationEvent.
// coordenadasConfirmacion EXISTE en el payload aunque su valor sea null (§9 del maestro).
public sealed record ConstanciaDto(
    TipoConstancia Tipo,
    string UrlEvidencia,
    string ReceptorNombre,
    DateTime FechaHora,
    CoordenadasDto? CoordenadasConfirmacion);
