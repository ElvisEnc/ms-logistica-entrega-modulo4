using Logistica.Application.Repartidores.Queries;
using Logistica.Application.Repartidores.RegistrarRepartidor;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Logistica.WebApi.Controllers;

// §11.1 DESIGN.md. El repartidor nace disponible (I11); "disponibles" filtra a los que no
// tienen ruta activa.
[ApiController]
[Route("api/repartidores")]
public sealed class RepartidoresController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Registrar(RegistrarRepartidorRequest request, CancellationToken ct)
    {
        var repartidorId = await sender.Send(
            new RegistrarRepartidorCommand(request.Nombre, request.Telefono, request.TipoVehiculo, request.Placa, request.CapacidadPaquetes),
            ct);

        return Ok(repartidorId);
    }

    [HttpGet]
    public async Task<ActionResult<RepartidorDto[]>> Listar([FromQuery] bool? disponibles, CancellationToken ct)
    {
        var repartidores = await sender.Send(new GetRepartidoresQuery(disponibles), ct);
        return Ok(repartidores);
    }
}

public sealed record RegistrarRepartidorRequest(
    string Nombre,
    string Telefono,
    string TipoVehiculo,
    string Placa,
    int CapacidadPaquetes);
