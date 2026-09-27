using Logistica.Application.Rutas.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Logistica.WebApi.Controllers;

// §11.6 DESIGN.md, HU-44. Respuesta operativa a un reclamo. "desde" y "hasta" son obligatorias;
// "pacienteId" y "pacienteNombre" son filtros opcionales, con pacienteId como correlacion
// prioritaria (D-04, §10.2).
[ApiController]
[Route("api/constancias")]
public sealed class ConstanciasController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HistorialConstanciaDto[]>> Listar(
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        [FromQuery] Guid? pacienteId,
        [FromQuery] string? pacienteNombre,
        CancellationToken ct)
    {
        var historial = await sender.Send(new GetHistorialConstanciasQuery(desde, hasta, pacienteId, pacienteNombre), ct);
        return Ok(historial);
    }
}
