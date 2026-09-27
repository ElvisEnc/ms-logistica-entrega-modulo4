using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

public sealed record Vehiculo
{
    public string Tipo { get; private set; } = string.Empty;
    public string Placa { get; private set; } = string.Empty;
    public int CapacidadPaquetes { get; private set; }

    private Vehiculo() { } // EF Core

    private Vehiculo(string tipo, string placa, int capacidadPaquetes)
    {
        Tipo = tipo;
        Placa = placa;
        CapacidadPaquetes = capacidadPaquetes;
    }

    public static Vehiculo Crear(string tipo, string placa, int capacidadPaquetes)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new DomainException(VehiculoErrors.TipoRequerido());

        if (string.IsNullOrWhiteSpace(placa))
            throw new DomainException(VehiculoErrors.PlacaRequerida());

        if (capacidadPaquetes <= 0)
            throw new DomainException(VehiculoErrors.CapacidadInvalida()); // RN-27

        return new Vehiculo(tipo, placa, capacidadPaquetes);
    }
}
