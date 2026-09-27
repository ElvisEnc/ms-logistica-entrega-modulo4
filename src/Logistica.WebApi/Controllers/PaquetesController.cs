using Logistica.Application.PaquetesRecibidos;
using Logistica.Application.PaquetesRecibidos.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Logistica.WebApi.Controllers;

// §11.2 DESIGN.md. Consulta que el administrador usa antes de crear una ruta, para saber que
// paquetes quedan por asignar.
[ApiController]
[Route("api/paquetes")]
public sealed class PaquetesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaqueteRecibidoDto[]>> Listar(
        [FromQuery] DateOnly fecha, [FromQuery] EstadoAsignacion? estado, CancellationToken ct)
    {
        var paquetes = await sender.Send(new GetPaquetesRecibidosQuery(fecha, estado), ct);
        return Ok(paquetes);
    }
}
