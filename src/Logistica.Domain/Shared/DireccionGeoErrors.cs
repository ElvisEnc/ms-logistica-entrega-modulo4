using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

public static class DireccionGeoErrors
{
    public static Error CalleRequerida() =>
        new("DIRECCION_CALLE_REQUERIDA", "La calle es obligatoria", ErrorType.Validation);

    public static Error ZonaRequerida() =>
        new("DIRECCION_ZONA_REQUERIDA", "La zona es obligatoria", ErrorType.Validation);

    public static Error CiudadRequerida() =>
        new("DIRECCION_CIUDAD_REQUERIDA", "La ciudad es obligatoria", ErrorType.Validation);

    public static Error CoordenadasRequeridas() =>
        new("DIRECCION_COORDENADAS_REQUERIDAS", "Las coordenadas son obligatorias", ErrorType.Validation);
}
