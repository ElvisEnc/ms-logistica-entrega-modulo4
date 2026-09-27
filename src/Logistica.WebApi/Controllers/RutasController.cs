using Logistica.Application.Rutas.CancelarRuta;
using Logistica.Application.Rutas.CompletarRuta;
using Logistica.Application.Rutas.CrearRuta;
using Logistica.Application.Rutas.IniciarRuta;
using Logistica.Application.Rutas.OptimizarRuta;
using Logistica.Application.Rutas.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Logistica.WebApi.Controllers;

// §11.3 DESIGN.md. El origen del recorrido viaja en el cuerpo de "optimizar", no en la ruta ni
// en configuracion (§5.7). El mapa devuelve text/html, no JSON: es material de demostracion de
// RN-15 y no toca el dominio (§11.3).
[ApiController]
[Route("api/rutas")]
public sealed class RutasController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Crear(CrearRutaRequest request, CancellationToken ct)
    {
        var rutaId = await sender.Send(new CrearRutaCommand(request.Fecha, request.RepartidorId, request.PaqueteIds), ct);
        return Ok(rutaId);
    }

    [HttpPost("{id:guid}/optimizar")]
    public async Task<IActionResult> Optimizar(Guid id, OptimizarRutaRequest request, CancellationToken ct)
    {
        await sender.Send(
            new OptimizarRutaCommand(id, request.Calle, request.Zona, request.Ciudad, request.Referencia, request.Latitud, request.Longitud),
            ct);

        return Ok();
    }

    [HttpPost("{id:guid}/iniciar")]
    public async Task<IActionResult> Iniciar(Guid id, CancellationToken ct)
    {
        await sender.Send(new IniciarRutaCommand(id), ct);
        return Ok();
    }

    [HttpPost("{id:guid}/completar")]
    public async Task<IActionResult> Completar(Guid id, CancellationToken ct)
    {
        await sender.Send(new CompletarRutaCommand(id), ct);
        return Ok();
    }

    [HttpPost("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id, CancelarRutaRequest request, CancellationToken ct)
    {
        await sender.Send(new CancelarRutaCommand(id, request.Motivo), ct);
        return Ok();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RutaDetalleDto>> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var ruta = await sender.Send(new GetRutaByIdQuery(id), ct);
        return Ok(ruta);
    }

    [HttpGet]
    public async Task<ActionResult<RutaDetalleDto>> ObtenerDelDia([FromQuery] Guid repartidorId, [FromQuery] DateOnly fecha, CancellationToken ct)
    {
        var ruta = await sender.Send(new GetRutaDelDiaQuery(repartidorId, fecha), ct);
        return Ok(ruta);
    }

    [HttpGet("{id:guid}/mapa")]
    public async Task<IActionResult> Mapa(Guid id, CancellationToken ct)
    {
        var html = await sender.Send(new GetMapaRutaQuery(id), ct);
        return Content(html, "text/html");
    }
}

public sealed record CrearRutaRequest(DateOnly Fecha, Guid RepartidorId, IReadOnlyCollection<Guid> PaqueteIds);

public sealed record OptimizarRutaRequest(string Calle, string Zona, string Ciudad, string? Referencia, decimal Latitud, decimal Longitud);

public sealed record CancelarRutaRequest(string Motivo);
