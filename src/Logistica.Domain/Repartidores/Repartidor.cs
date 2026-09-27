using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Repartidores;

public sealed class Repartidor : AggregateRoot
{
    public RepartidorId RepartidorId => RepartidorId.From(Id);
    public string Nombre { get; private set; } = string.Empty;
    public string Telefono { get; private set; } = string.Empty;
    public Vehiculo Vehiculo { get; private set; } = null!;
    public bool Disponible { get; private set; }

    private Repartidor() { } // EF Core

    private Repartidor(Guid id, string nombre, string telefono, Vehiculo vehiculo) : base(id)
    {
        Nombre = nombre;
        Telefono = telefono;
        Vehiculo = vehiculo;
        Disponible = true;
    }

    public static Repartidor Registrar(string nombre, string telefono, Vehiculo vehiculo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException(RepartidorErrors.NombreRequerido());

        if (string.IsNullOrWhiteSpace(telefono))
            throw new DomainException(RepartidorErrors.TelefonoRequerido());

        if (vehiculo is null)
            throw new DomainException(RepartidorErrors.VehiculoRequerido()); // RN-27

        return new Repartidor(Guid.NewGuid(), nombre, telefono, vehiculo);
    }

    public bool EstaDisponible() => Disponible;

    // I2 (RN-27, RN-13) (mitad de capacidad): comparacion pura, Application la consulta antes de decidir
    public bool TieneCapacidadPara(int cantidadPaquetes) => cantidadPaquetes <= Vehiculo.CapacidadPaquetes;

    public void AsignarRuta()
    {
        if (!Disponible)
            throw new DomainException(RepartidorErrors.NoDisponible()); // I2 (RN-27) (mitad de disponibilidad), refuerzo de I11 (RN-27)

        Disponible = false;
    }

    // Idempotente: liberar a alguien ya disponible no lanza (§5.2)
    public void Liberar() => Disponible = true; // I11 (RN-27)
}
