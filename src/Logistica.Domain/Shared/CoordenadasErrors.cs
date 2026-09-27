using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

public static class CoordenadasErrors
{
    public static Error LatitudInvalida() =>
        new("COORDENADAS_LATITUD_INVALIDA", "La latitud debe estar entre -90 y 90 grados", ErrorType.Validation);

    public static Error LongitudInvalida() =>
        new("COORDENADAS_LONGITUD_INVALIDA", "La longitud debe estar entre -180 y 180 grados", ErrorType.Validation);
}
