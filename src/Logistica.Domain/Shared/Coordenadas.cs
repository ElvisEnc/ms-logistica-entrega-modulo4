using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

public sealed record Coordenadas
{
    public decimal Latitud { get; private set; }
    public decimal Longitud { get; private set; }

    private Coordenadas() { } // EF Core

    private Coordenadas(decimal latitud, decimal longitud)
    {
        Latitud = latitud;
        Longitud = longitud;
    }

    public static Coordenadas Crear(decimal latitud, decimal longitud)
    {
        if (latitud is < -90 or > 90)
            throw new DomainException(CoordenadasErrors.LatitudInvalida());

        if (longitud is < -180 or > 180)
            throw new DomainException(CoordenadasErrors.LongitudInvalida());

        return new Coordenadas(latitud, longitud);
    }

    // D-05 (§19.4 del DESIGN.md): Clonar() en todos los VO de varios campos, por conformidad con §12.3 del maestro
    public Coordenadas Clonar() => new(Latitud, Longitud);
}
