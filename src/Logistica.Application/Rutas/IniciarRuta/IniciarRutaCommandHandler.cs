using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Logistica.Domain.Rutas;
using MediatR;

namespace Logistica.Application.Rutas.IniciarRuta;

// Ninguna invariante de orquestacion (§10.1 DESIGN.md): I3 vive en el agregado.
internal sealed class IniciarRutaCommandHandler(IRutaEntregaRepository rutas, IUnitOfWork unitOfWork)
    : IRequestHandler<IniciarRutaCommand>
{
    public async Task Handle(IniciarRutaCommand request, CancellationToken cancellationToken)
    {
        var ruta = await rutas.GetByIdAsync(request.RutaId)
            ?? throw new DomainException(RutaErrors.RutaNoEncontrada());

        ruta.Iniciar(); // I3

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
