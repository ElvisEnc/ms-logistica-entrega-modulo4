namespace Logistica.Application.IntegrationEvents;

// SISTEMA.md §9.11 — BC5 -> BC6. §11.8 DESIGN.md: el DTO del endpoint POST
// /api/integracion/paquetes-listos y el contrato del evento son el MISMO tipo, sin
// traduccion intermedia. Sin sufijo IntegrationEvent: BC6 no tiene un evento de dominio
// homonimo con el que pueda colisionar (a diferencia de BC5, §9 del maestro).
public sealed record PaquetesListosParaEntrega(
    DateOnly Fecha,
    IReadOnlyCollection<PaqueteListoDto> Paquetes);
