using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Logistica.Application.Abstractions;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;
using MediatR;

namespace Logistica.Application.Rutas.ConfirmarEntrega;

// §10.1 DESIGN.md: la unica invariante que corresponde aqui es EVIDENCIA_ARCHIVO_REQUERIDO
// (RN-16). I4 e I5 se delegan integros en el dominio. El guard de extension/tamano (D-09,
// INC-1 Opcion A) vive en EvidenciaArchivoValidator.
internal sealed class ConfirmarEntregaCommandHandler(
    IRutaEntregaRepository rutas,
    IEvidenciaStorage evidencias,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ConfirmarEntregaCommand>
{
    public async Task Handle(ConfirmarEntregaCommand request, CancellationToken cancellationToken)
    {
        // Se comprueba ANTES de tocar el repositorio (§10.1): no tiene sentido ir a la base de
        // datos para rechazar la peticion.
        if (request.Archivo is null)
            throw new DomainException(RutaErrors.EvidenciaArchivoRequerido()); // RN-16

        EvidenciaArchivoValidator.Validar(request.Archivo, request.NombreArchivo); // D-09, INC-1 Opcion A

        var ruta = await rutas.GetByIdAsync(request.RutaId)
            ?? throw new DomainException(RutaErrors.RutaNoEncontrada());

        // El archivo se guarda PRIMERO y solo despues se construye el VO con la URL resultante
        // (§12.2): si el dominio rechaza la transicion mas abajo, el archivo queda huerfano en
        // disco a proposito — es mas barato que una transaccion distribuida disco+PostgreSQL.
        var urlEvidencia = await evidencias.GuardarAsync(
            request.Archivo, request.ParadaId, request.NombreArchivo ?? string.Empty, cancellationToken);

        Coordenadas? coordenadasConfirmacion = request.Latitud is not null && request.Longitud is not null
            ? Coordenadas.Crear(request.Latitud.Value, request.Longitud.Value)
            : null;

        var constancia = ConstanciaEntrega.Crear(
            DateTime.UtcNow, request.Tipo, urlEvidencia, request.ReceptorNombre, coordenadasConfirmacion);

        ruta.ConfirmarEntrega(ParadaId.From(request.ParadaId), constancia); // I4, I5 en Domain

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
