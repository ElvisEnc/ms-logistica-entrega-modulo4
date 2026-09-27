using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

public static class SharedErrors
{
    public static Error IdVacio() =>
        new("ID_VACIO", "El identificador no puede estar vacio", ErrorType.Validation);
}
