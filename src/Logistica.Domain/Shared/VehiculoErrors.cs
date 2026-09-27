using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

public static class VehiculoErrors
{
    public static Error TipoRequerido() =>
        new("VEHICULO_TIPO_REQUERIDO", "El tipo de vehiculo es obligatorio", ErrorType.Validation);

    public static Error PlacaRequerida() =>
        new("VEHICULO_PLACA_REQUERIDA", "La placa del vehiculo es obligatoria", ErrorType.Validation);

    public static Error CapacidadInvalida() =>
        new("VEHICULO_CAPACIDAD_INVALIDA", "La capacidad de paquetes debe ser mayor a cero", ErrorType.Validation);
}
