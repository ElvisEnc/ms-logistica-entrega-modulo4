namespace Logistica.Domain.Shared;

public sealed record ContratoId
{
    public Guid Value { get; }

    private ContratoId(Guid value) => Value = value;

    public static ContratoId New() => new(Guid.NewGuid());

    public static ContratoId From(Guid value)
    {
        TypedIdValidator.Validar(value);
        return new ContratoId(value);
    }
}
