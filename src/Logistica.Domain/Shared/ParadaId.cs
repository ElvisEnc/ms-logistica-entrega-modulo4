namespace Logistica.Domain.Shared;

public sealed record ParadaId
{
    public Guid Value { get; }

    private ParadaId(Guid value) => Value = value;

    public static ParadaId New() => new(Guid.NewGuid());

    public static ParadaId From(Guid value)
    {
        TypedIdValidator.Validar(value);
        return new ParadaId(value);
    }
}
