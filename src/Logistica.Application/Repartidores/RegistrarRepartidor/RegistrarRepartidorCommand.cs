using MediatR;

namespace Logistica.Application.Repartidores.RegistrarRepartidor;

internal sealed record RegistrarRepartidorCommand(
    string Nombre,
    string Telefono,
    string TipoVehiculo,
    string Placa,
    int CapacidadPaquetes) : IRequest<Guid>;
