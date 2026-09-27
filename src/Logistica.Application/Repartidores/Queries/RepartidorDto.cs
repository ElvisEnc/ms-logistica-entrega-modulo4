namespace Logistica.Application.Repartidores.Queries;

public sealed record RepartidorDto(
    Guid RepartidorId,
    string Nombre,
    string Telefono,
    string TipoVehiculo,
    string Placa,
    int CapacidadPaquetes,
    bool Disponible);
