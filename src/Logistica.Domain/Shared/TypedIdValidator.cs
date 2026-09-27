using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

internal static class TypedIdValidator
{
    public static void Validar(Guid value)
    {
        if (value == Guid.Empty)
            throw new DomainException(SharedErrors.IdVacio());
    }
}
