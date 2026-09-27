using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;
using MediatR;

namespace Logistica.Application.Rutas.OptimizarRuta;

// Ninguna invariante de orquestacion (§10.1 DESIGN.md): I3 e I8 viven integras en el agregado.
internal sealed class OptimizarRutaCommandHandler(IRutaEntregaRepository rutas, IUnitOfWork unitOfWork)
    : IRequestHandler<OptimizarRutaCommand>
{
    public async Task Handle(OptimizarRutaCommand request, CancellationToken cancellationToken)
    {
        var ruta = await rutas.GetByIdAsync(request.RutaId)
            ?? throw new DomainException(RutaErrors.RutaNoEncontrada());

        var origen = DireccionGeo.Crear(
            request.Calle,
            request.Zona,
            request.Ciudad,
            request.Referencia,
            Coordenadas.Crear(request.Latitud, request.Longitud));

        ruta.Optimizar(origen); // I3, I8

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
