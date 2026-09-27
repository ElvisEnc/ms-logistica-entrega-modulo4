using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Rutas;

public static class ParadaEntregaErrors
{
    public static Error TransicionInvalida() =>
        new("I4_TRANSICION_INVALIDA", "La parada ya esta resuelta y no admite una nueva transicion", ErrorType.Conflict); // I4 (RN-16)

    public static Error ConstanciaRequerida() =>
        new("I5_CONSTANCIA_REQUERIDA", "La constancia de entrega es obligatoria para confirmar la entrega", ErrorType.Validation); // I5 (RN-16)

    public static Error IncidenciaRequerida() =>
        new("I5_INCIDENCIA_REQUERIDA", "La incidencia es obligatoria para reportar la entrega", ErrorType.Validation); // I5 (RN-16)

    public static Error OrdenInvalido() =>
        new("I8_ORDEN_PARADAS_INVALIDO", "El orden de la parada debe ser mayor o igual a 1", ErrorType.Validation); // I8 (RN-13, RN-15)
}
