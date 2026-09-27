using Logistica.Application.Rutas.ConfirmarEntrega;
using Logistica.Application.Rutas.ReportarIncidencia;
using Logistica.Domain.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Logistica.WebApi.Controllers;

// §11.4 DESIGN.md. Los DOS UNICOS endpoints multipart/form-data del microservicio. El archivo es
// OBLIGATORIO al confirmar y OPCIONAL al reportar incidencia (decision de negocio, §11.4): el
// controller solo abre el stream y lo pasa con el nombre original, la obligatoriedad la valida
// el handler de Application (EVIDENCIA_ARCHIVO_REQUERIDO), nunca aqui.
[ApiController]
[Route("api/rutas/{rutaId:guid}/paradas/{paradaId:guid}")]
public sealed class ParadasController(ISender sender) : ControllerBase
{
    [HttpPost("confirmar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Confirmar(Guid rutaId, Guid paradaId, [FromForm] ConfirmarEntregaRequest request, CancellationToken ct)
    {
        await sender.Send(
            new ConfirmarEntregaCommand(
                rutaId,
                paradaId,
                request.Tipo,
                request.Archivo?.OpenReadStream(),
                request.Archivo?.FileName,
                request.ReceptorNombre,
                request.Latitud,
                request.Longitud),
            ct);

        return Ok();
    }

    [HttpPost("incidencia")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ReportarIncidencia(Guid rutaId, Guid paradaId, [FromForm] ReportarIncidenciaRequest request, CancellationToken ct)
    {
        await sender.Send(
            new ReportarIncidenciaCommand(
                rutaId,
                paradaId,
                request.Motivo,
                request.Descripcion,
                request.Archivo?.OpenReadStream(),
                request.Archivo?.FileName),
            ct);

        return Ok();
    }
}

public sealed class ConfirmarEntregaRequest
{
    public TipoConstancia Tipo { get; set; }
    public IFormFile? Archivo { get; set; }
    public string ReceptorNombre { get; set; } = string.Empty;
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}

public sealed class ReportarIncidenciaRequest
{
    public MotivoIncidencia Motivo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public IFormFile? Archivo { get; set; }
}
