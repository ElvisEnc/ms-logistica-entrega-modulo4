using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Logistica.Domain.Rutas;
using MediatR;

namespace Logistica.Application.Rutas.CancelarRuta;

// Ninguna invariante de orquestacion en este handler (§10.1 DESIGN.md): I13 vive en el
// agregado; I9 e I11 las sostienen las politicas que reaccionan a RutaCancelada.
internal sealed class CancelarRutaCommandHandler(IRutaEntregaRepository rutas, IUnitOfWork unitOfWork)
    : IRequestHandler<CancelarRutaCommand>
{
    public async Task Handle(CancelarRutaCommand request, CancellationToken cancellationToken)
    {
        var ruta = await rutas.GetByIdAsync(request.RutaId)
            ?? throw new DomainException(RutaErrors.RutaNoEncontrada());

        ruta.Cancelar(request.Motivo); // I13

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
