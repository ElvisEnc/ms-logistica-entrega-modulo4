namespace Logistica.Domain.Shared;

public sealed record RepartidorId
{
    public Guid Value { get; }

    private RepartidorId(Guid value) => Value = value;

    public static RepartidorId New() => new(Guid.NewGuid());

    public static RepartidorId From(Guid value)
    {
        TypedIdValidator.Validar(value);
        return new RepartidorId(value);
    }
}
