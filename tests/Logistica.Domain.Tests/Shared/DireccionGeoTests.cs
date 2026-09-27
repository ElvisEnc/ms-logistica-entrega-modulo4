using Joseco.DDD.Core.Results;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Tests.Shared;

[Trait("Capa", "Unit")]
public class DireccionGeoTests
{
    [Fact]
    public void Crear_con_calle_vacia_lanza_DIRECCION_CALLE_REQUERIDA()
    {
        // Arrange
        var coordenadas = Coordenadas.Crear(-17.783206m, -63.182140m);

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            DireccionGeo.Crear(string.Empty, "Equipetrol", "Santa Cruz de la Sierra", null, coordenadas));

        // Assert
        Assert.Equal("DIRECCION_CALLE_REQUERIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Crear_con_zona_vacia_lanza_DIRECCION_ZONA_REQUERIDA()
    {
        // Arrange
        var coordenadas = Coordenadas.Crear(-17.783206m, -63.182140m);

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            DireccionGeo.Crear("Av. San Martín", string.Empty, "Santa Cruz de la Sierra", null, coordenadas));

        // Assert
        Assert.Equal("DIRECCION_ZONA_REQUERIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Crear_con_ciudad_vacia_lanza_DIRECCION_CIUDAD_REQUERIDA()
    {
        // Arrange
        var coordenadas = Coordenadas.Crear(-17.783206m, -63.182140m);

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            DireccionGeo.Crear("Av. San Martín", "Equipetrol", string.Empty, null, coordenadas));

        // Assert
        Assert.Equal("DIRECCION_CIUDAD_REQUERIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Crear_con_coordenadas_nulas_lanza_DIRECCION_COORDENADAS_REQUERIDAS()
    {
        // Arrange
        Coordenadas coordenadas = null!;

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            DireccionGeo.Crear("Av. San Martín", "Equipetrol", "Santa Cruz de la Sierra", null, coordenadas)); // RN-15

        // Assert
        Assert.Equal("DIRECCION_COORDENADAS_REQUERIDAS", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Frente al parque")]
    public void Referencia_es_opcional(string? referencia)
    {
        // Arrange
        var coordenadas = Coordenadas.Crear(-17.783206m, -63.182140m);

        // Act
        var direccion = DireccionGeo.Crear("Av. San Martín", "Equipetrol", "Santa Cruz de la Sierra", referencia, coordenadas);

        // Assert
        Assert.Equal(referencia, direccion.Referencia);
    }

    [Fact]
    public void DistanciaHasta_el_mismo_punto_es_cero()
    {
        // Arrange
        var coordenadas = Coordenadas.Crear(-17.783206m, -63.182140m);
        var origen = DireccionGeo.Crear("Av. San Martín", "Equipetrol", "Santa Cruz de la Sierra", null, coordenadas);
        var mismoPunto = DireccionGeo.Crear("Av. San Martín", "Equipetrol", "Santa Cruz de la Sierra", null, coordenadas.Clonar());

        // Act
        var distancia = origen.DistanciaHasta(mismoPunto);

        // Assert
        Assert.Equal(0m, distancia);
    }

    [Fact]
    public void DistanciaHasta_un_grado_de_latitud_en_el_ecuador_da_aproximadamente_111_19_km()
    {
        // Arrange
        var origen = DireccionGeo.Crear("Calle 1", "Zona A", "Ciudad X", null, Coordenadas.Crear(0m, 0m));
        var destino = DireccionGeo.Crear("Calle 2", "Zona B", "Ciudad X", null, Coordenadas.Crear(1m, 0m));

        // Act
        var distancia = origen.DistanciaHasta(destino);

        // Assert
        Assert.Equal(111.19m, distancia, 2);
    }

    [Fact]
    public void Clonar_produce_una_instancia_equivalente_pero_con_Coordenadas_propia()
    {
        // Arrange
        var coordenadas = Coordenadas.Crear(-17.783206m, -63.182140m);
        var original = DireccionGeo.Crear("Av. San Martín", "Equipetrol", "Santa Cruz de la Sierra", "Frente al parque", coordenadas);

        // Act
        var clon = original.Clonar();

        // Assert
        Assert.Equal(original, clon);
        Assert.NotSame(original.Coordenadas, clon.Coordenadas);
    }
}
