using Logistica.Application.IntegrationEvents;
using Logistica.Domain.Shared;

namespace Logistica.Application.Rutas.Queries;

public sealed record ConstanciaEntregaDto(
    TipoConstancia Tipo,
    string UrlEvidencia,
    string ReceptorNombre,
    DateTime FechaHora,
    CoordenadasDto? CoordenadasConfirmacion);
