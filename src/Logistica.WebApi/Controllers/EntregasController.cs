using Logistica.Application.Rutas.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Logistica.WebApi.Controllers;

// §11.5 DESIGN.md, HU-42. Estado en vivo: una fila por parada de todas las rutas del dia. Lista
// vacia si no hay rutas ese dia, nunca 404 (§10.2).
[ApiController]
[Route("api/entregas")]
public sealed class EntregasController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<EstadoEntregaDto[]>> Listar([FromQuery] DateOnly fecha, CancellationToken ct)
    {
        var estados = await sender.Send(new GetEstadoEntregasQuery(fecha), ct);
        return Ok(estados);
    }
}
