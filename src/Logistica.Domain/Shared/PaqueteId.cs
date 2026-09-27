namespace Logistica.Domain.Shared;

public sealed record PaqueteId
{
    public Guid Value { get; }

    private PaqueteId(Guid value) => Value = value;

    public static PaqueteId New() => new(Guid.NewGuid());

    public static PaqueteId From(Guid value)
    {
        TypedIdValidator.Validar(value);
        return new PaqueteId(value);
    }
}
