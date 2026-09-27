using Logistica.Domain.Shared;
using MediatR;

namespace Logistica.Application.Rutas.ConfirmarEntrega;

// Uno de los dos unicos comandos que transportan un stream (§10.1 DESIGN.md): la evidencia es
// un archivo real. El archivo es OBLIGATORIO aqui, a diferencia de ReportarIncidenciaCommand.
internal sealed record ConfirmarEntregaCommand(
    Guid RutaId,
    Guid ParadaId,
    TipoConstancia Tipo,
    Stream? Archivo,
    string? NombreArchivo,
    string ReceptorNombre,
    decimal? Latitud,
    decimal? Longitud) : IRequest;
