using Logistica.Application.IntegrationEvents;
using Logistica.Application.PaquetesRecibidos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Logistica.WebApi.Controllers;

// §11.8 DESIGN.md, §9.11 del maestro. Unico endpoint de integracion entrante de BC6, idempotente
// (I12): reenviar el mismo lote responde 200 igual que la primera vez. El cuerpo se enlaza
// directamente al record del contrato, sin traduccion intermedia (§11.8).
[ApiController]
[Route("api/integracion")]
public sealed class IntegracionController(ISender sender) : ControllerBase
{
    [HttpPost("paquetes-listos")]
    public async Task<IActionResult> PaquetesListos(PaquetesListosParaEntrega contrato, CancellationToken ct)
    {
        await sender.Send(new ProcesarPaquetesListosCommand(contrato), ct);
        return Ok();
    }
}
