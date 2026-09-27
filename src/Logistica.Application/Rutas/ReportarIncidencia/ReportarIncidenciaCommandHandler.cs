using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Logistica.Application.Abstractions;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;
using MediatR;

namespace Logistica.Application.Rutas.ReportarIncidencia;

// Ninguna invariante de orquestacion (§10.1 DESIGN.md): el archivo es opcional aqui. I4, I5 e
// I14 se delegan integros en el dominio. El guard de extension/tamano (D-09, INC-1 Opcion A)
// vive en EvidenciaArchivoValidator y solo aplica si llega archivo.
internal sealed class ReportarIncidenciaCommandHandler(
    IRutaEntregaRepository rutas,
    IEvidenciaStorage evidencias,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ReportarIncidenciaCommand>
{
    public async Task Handle(ReportarIncidenciaCommand request, CancellationToken cancellationToken)
    {
        var ruta = await rutas.GetByIdAsync(request.RutaId)
            ?? throw new DomainException(RutaErrors.RutaNoEncontrada());

        string? urlFoto = null;
        if (request.Archivo is not null)
        {
            EvidenciaArchivoValidator.Validar(request.Archivo, request.NombreArchivo); // D-09, INC-1 Opcion A

            urlFoto = await evidencias.GuardarAsync(
                request.Archivo, request.ParadaId, request.NombreArchivo ?? string.Empty, cancellationToken);
        }

        var incidencia = IncidenciaEntrega.Crear(DateTime.UtcNow, request.Motivo, request.Descripcion, urlFoto);

        ruta.ReportarIncidencia(ParadaId.From(request.ParadaId), incidencia); // I4, I5, I14 en Domain

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
