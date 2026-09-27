using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

public static class IncidenciaEntregaErrors
{
    public static Error DescripcionRequerida() =>
        new("INCIDENCIA_DESCRIPCION_REQUERIDA", "La descripcion de la incidencia es obligatoria", ErrorType.Validation);
}
