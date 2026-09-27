using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Logistica.Domain.Rutas;
using MediatR;

namespace Logistica.Application.Rutas.CompletarRuta;

// Ninguna invariante de orquestacion en este handler (§10.1 DESIGN.md): I6 e I7 viven en el
// agregado; I11 la sostiene LiberarRepartidorAlCompletarRutaPolicy, reaccionando a RutaCompletada.
internal sealed class CompletarRutaCommandHandler(IRutaEntregaRepository rutas, IUnitOfWork unitOfWork)
    : IRequestHandler<CompletarRutaCommand>
{
    public async Task Handle(CompletarRutaCommand request, CancellationToken cancellationToken)
    {
        var ruta = await rutas.GetByIdAsync(request.RutaId)
            ?? throw new DomainException(RutaErrors.RutaNoEncontrada());

        ruta.Completar(); // I6, I7

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
