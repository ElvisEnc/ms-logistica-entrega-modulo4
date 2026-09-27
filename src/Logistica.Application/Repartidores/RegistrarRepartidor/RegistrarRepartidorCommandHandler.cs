using Joseco.DDD.Core.Abstractions;
using Logistica.Domain.Repartidores;
using Logistica.Domain.Shared;
using MediatR;

namespace Logistica.Application.Repartidores.RegistrarRepartidor;

// Ninguna invariante de orquestacion (§10.1 DESIGN.md): las validaciones son del constructor
// de Repartidor y de Vehiculo.
internal sealed class RegistrarRepartidorCommandHandler(IRepartidorRepository repartidores, IUnitOfWork unitOfWork)
    : IRequestHandler<RegistrarRepartidorCommand, Guid>
{
    public async Task<Guid> Handle(RegistrarRepartidorCommand request, CancellationToken cancellationToken)
    {
        var vehiculo = Vehiculo.Crear(request.TipoVehiculo, request.Placa, request.CapacidadPaquetes);
        var repartidor = Repartidor.Registrar(request.Nombre, request.Telefono, vehiculo);

        await repartidores.AddAsync(repartidor);
        await unitOfWork.CommitAsync(cancellationToken);

        return repartidor.RepartidorId.Value;
    }
}
