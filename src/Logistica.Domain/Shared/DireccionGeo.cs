using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

public sealed record DireccionGeo
{
    private const double RadioTierraKm = 6371.0;

    public string Calle { get; private set; } = string.Empty;
    public string Zona { get; private set; } = string.Empty;
    public string Ciudad { get; private set; } = string.Empty;
    public string? Referencia { get; private set; }
    public Coordenadas Coordenadas { get; private set; } = null!;

    private DireccionGeo() { } // EF Core

    private DireccionGeo(string calle, string zona, string ciudad, string? referencia, Coordenadas coordenadas)
    {
        Calle = calle;
        Zona = zona;
        Ciudad = ciudad;
        Referencia = referencia;
        Coordenadas = coordenadas;
    }

    public static DireccionGeo Crear(string calle, string zona, string ciudad, string? referencia, Coordenadas coordenadas)
    {
        if (string.IsNullOrWhiteSpace(calle))
            throw new DomainException(DireccionGeoErrors.CalleRequerida());

        if (string.IsNullOrWhiteSpace(zona))
            throw new DomainException(DireccionGeoErrors.ZonaRequerida());

        if (string.IsNullOrWhiteSpace(ciudad))
            throw new DomainException(DireccionGeoErrors.CiudadRequerida()); // RN-15

        if (coordenadas is null)
            throw new DomainException(DireccionGeoErrors.CoordenadasRequeridas()); // RN-15

        return new DireccionGeo(calle, zona, ciudad, referencia, coordenadas);
    }

    // RN-15, HU-38, §12.4: haversine sobre R = 6371 km
    public decimal DistanciaHasta(DireccionGeo otra)
    {
        var lat1 = DegreesToRadians((double)Coordenadas.Latitud);
        var lat2 = DegreesToRadians((double)otra.Coordenadas.Latitud);
        var deltaPhi = DegreesToRadians((double)(otra.Coordenadas.Latitud - Coordenadas.Latitud));
        var deltaLambda = DegreesToRadians((double)(otra.Coordenadas.Longitud - Coordenadas.Longitud));

        var a = Math.Pow(Math.Sin(deltaPhi / 2), 2) +
                Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLambda / 2), 2);

        var distanciaKm = 2 * RadioTierraKm * Math.Asin(Math.Sqrt(a));

        return (decimal)distanciaKm;
    }

    // Regla de owned types (§12.3 del maestro): una instancia no se comparte entre dos dueños
    public DireccionGeo Clonar() =>
        new(Calle, Zona, Ciudad, Referencia, Coordenadas.Clonar());

    private static double DegreesToRadians(double grados) => grados * Math.PI / 180.0;
}
