namespace Logistica.Domain.Shared;

public sealed record PacienteId
{
    public Guid Value { get; }

    private PacienteId(Guid value) => Value = value;

    public static PacienteId New() => new(Guid.NewGuid());

    public static PacienteId From(Guid value)
    {
        TypedIdValidator.Validar(value);
        return new PacienteId(value);
    }
}
