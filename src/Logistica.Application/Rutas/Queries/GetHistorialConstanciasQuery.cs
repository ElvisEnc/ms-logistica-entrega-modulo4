using MediatR;

namespace Logistica.Application.Rutas.Queries;

// HU-44. Filtra prioritariamente por PacienteId; PacienteNombre es una comodidad de la mesa de
// reclamos, coincidencia parcial sin distinguir mayusculas, y NO es la correlacion oficial
// (§9.3, §10.2 DESIGN.md). El handler valida Desde <= Hasta y lanza RANGO_FECHAS_INVALIDO si no.
public sealed record GetHistorialConstanciasQuery(
    DateOnly Desde,
    DateOnly Hasta,
    Guid? PacienteId,
    string? PacienteNombre) : IRequest<HistorialConstanciaDto[]>;
