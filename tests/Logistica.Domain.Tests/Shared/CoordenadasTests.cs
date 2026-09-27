using Joseco.DDD.Core.Results;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Tests.Shared;

[Trait("Capa", "Unit")]
public class CoordenadasTests
{
    [Theory]
    [InlineData(-90)]
    [InlineData(90)]
    public void Crear_con_latitud_en_el_limite_90_o_menos_90_es_valida(double latitudLimite)
    {
        // Arrange
        var latitud = (decimal)latitudLimite;
        var longitud = 0m;

        // Act
        var coordenadas = Coordenadas.Crear(latitud, longitud);

        // Assert
        Assert.Equal(latitud, coordenadas.Latitud);
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    [InlineData(-1000)]
    [InlineData(1000)]
    public void Crear_con_latitud_fuera_de_rango_lanza_COORDENADAS_LATITUD_INVALIDA(double latitudFueraDeRango)
    {
        // Arrange
        var latitud = (decimal)latitudFueraDeRango;
        var longitud = 0m;

        // Act
        var excepcion = Assert.Throws<DomainException>(() => Coordenadas.Crear(latitud, longitud));

        // Assert
        Assert.Equal("COORDENADAS_LATITUD_INVALIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Theory]
    [InlineData(-180)]
    [InlineData(180)]
    public void Crear_con_longitud_en_el_limite_180_o_menos_180_es_valida(double longitudLimite)
    {
        // Arrange
        var latitud = 0m;
        var longitud = (decimal)longitudLimite;

        // Act
        var coordenadas = Coordenadas.Crear(latitud, longitud);

        // Assert
        Assert.Equal(longitud, coordenadas.Longitud);
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    [InlineData(-1000)]
    [InlineData(1000)]
    public void Crear_con_longitud_fuera_de_rango_lanza_COORDENADAS_LONGITUD_INVALIDA(double longitudFueraDeRango)
    {
        // Arrange
        var latitud = 0m;
        var longitud = (decimal)longitudFueraDeRango;

        // Act
        var excepcion = Assert.Throws<DomainException>(() => Coordenadas.Crear(latitud, longitud));

        // Assert
        Assert.Equal("COORDENADAS_LONGITUD_INVALIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }
}
