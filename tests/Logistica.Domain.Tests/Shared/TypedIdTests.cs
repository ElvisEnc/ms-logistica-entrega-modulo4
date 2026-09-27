using Joseco.DDD.Core.Results;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Tests.Shared;

[Trait("Capa", "Unit")]
public class TypedIdTests
{
    private static readonly Guid GuidFijo = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>
    /// Describe uno de los seis IDs tipados de `src/Logistica.Domain/Shared/`. Todos
    /// comparten la misma forma (`sealed record` con `New()`/`From(Guid)` que delega
    /// en `TypedIdValidator.Validar`), así que se parametriza en vez de repetir seis
    /// clases de test casi idénticas. `ToString` da el nombre legible en el Theory.
    /// </summary>
    public sealed class DescriptorTypedId
    {
        public required string Nombre { get; init; }
        public required Func<object> Nuevo { get; init; }
        public required Func<Guid, object> Desde { get; init; }
        public required Func<object, Guid> ValorDe { get; init; }

        public override string ToString() => Nombre;
    }

    public static TheoryData<DescriptorTypedId> TiposDeId() => new()
    {
        new DescriptorTypedId
        {
            Nombre = "RutaId",
            Nuevo = () => RutaId.New(),
            Desde = guid => RutaId.From(guid),
            ValorDe = id => ((RutaId)id).Value,
        },
        new DescriptorTypedId
        {
            Nombre = "ParadaId",
            Nuevo = () => ParadaId.New(),
            Desde = guid => ParadaId.From(guid),
            ValorDe = id => ((ParadaId)id).Value,
        },
        new DescriptorTypedId
        {
            Nombre = "RepartidorId",
            Nuevo = () => RepartidorId.New(),
            Desde = guid => RepartidorId.From(guid),
            ValorDe = id => ((RepartidorId)id).Value,
        },
        new DescriptorTypedId
        {
            Nombre = "PaqueteId",
            Nuevo = () => PaqueteId.New(),
            Desde = guid => PaqueteId.From(guid),
            ValorDe = id => ((PaqueteId)id).Value,
        },
        new DescriptorTypedId
        {
            Nombre = "PacienteId",
            Nuevo = () => PacienteId.New(),
            Desde = guid => PacienteId.From(guid),
            ValorDe = id => ((PacienteId)id).Value,
        },
        new DescriptorTypedId
        {
            Nombre = "ContratoId",
            Nuevo = () => ContratoId.New(),
            Desde = guid => ContratoId.From(guid),
            ValorDe = id => ((ContratoId)id).Value,
        },
    };

    [Theory]
    [MemberData(nameof(TiposDeId))]
    public void New_genera_un_id_valido(DescriptorTypedId id)
    {
        // Act
        var nuevoId = id.Nuevo();

        // Assert
        Assert.NotEqual(Guid.Empty, id.ValorDe(nuevoId));
    }

    [Theory]
    [MemberData(nameof(TiposDeId))]
    public void From_con_guid_valido_lo_envuelve(DescriptorTypedId id)
    {
        // Arrange
        var guidValido = GuidFijo;

        // Act
        var envuelto = id.Desde(guidValido);

        // Assert
        Assert.Equal(guidValido, id.ValorDe(envuelto));
    }

    [Theory]
    [MemberData(nameof(TiposDeId))]
    public void From_con_Guid_Empty_lanza_ID_VACIO(DescriptorTypedId id)
    {
        // Arrange
        var guidVacio = Guid.Empty;

        // Act
        var excepcion = Assert.Throws<DomainException>(() => id.Desde(guidVacio));

        // Assert
        Assert.Equal("ID_VACIO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Theory]
    [MemberData(nameof(TiposDeId))]
    public void Dos_ids_con_el_mismo_Guid_son_iguales_por_valor(DescriptorTypedId id)
    {
        // Arrange
        var guidCompartido = GuidFijo;

        // Act
        var primero = id.Desde(guidCompartido);
        var segundo = id.Desde(guidCompartido);

        // Assert
        Assert.Equal(primero, segundo);
        Assert.Equal(primero.GetHashCode(), segundo.GetHashCode());
    }
}
