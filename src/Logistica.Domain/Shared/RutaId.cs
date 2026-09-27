namespace Logistica.Domain.Shared;

public sealed record RutaId
{
    public Guid Value { get; }

    private RutaId(Guid value) => Value = value;

    public static RutaId New() => new(Guid.NewGuid());

    public static RutaId From(Guid value)
    {
        TypedIdValidator.Validar(value);
        return new RutaId(value);
    }
}
