using Logistica.Domain.Shared;
using MediatR;

namespace Logistica.Application.Rutas.ReportarIncidencia;

// El archivo es OPCIONAL aqui (§10.1, §18.3 DESIGN.md): decision de negocio, no una omision.
internal sealed record ReportarIncidenciaCommand(
    Guid RutaId,
    Guid ParadaId,
    MotivoIncidencia Motivo,
    string Descripcion,
    Stream? Archivo,
    string? NombreArchivo) : IRequest;
