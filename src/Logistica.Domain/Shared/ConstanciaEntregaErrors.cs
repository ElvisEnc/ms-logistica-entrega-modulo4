using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

public static class ConstanciaEntregaErrors
{
    public static Error UrlRequerida() =>
        new("CONSTANCIA_URL_REQUERIDA", "La URL de la evidencia es obligatoria", ErrorType.Validation); // RN-16

    public static Error ReceptorRequerido() =>
        new("CONSTANCIA_RECEPTOR_REQUERIDO", "El nombre del receptor es obligatorio", ErrorType.Validation); // RN-16
}
