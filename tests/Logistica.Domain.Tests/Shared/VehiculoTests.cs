using Joseco.DDD.Core.Results;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Tests.Shared;

[Trait("Capa", "Unit")]
public class VehiculoTests
{
    [Fact]
    public void Crear_con_tipo_vacio_lanza_VEHICULO_TIPO_REQUERIDO()
    {
        // Arrange
        var tipo = string.Empty;
        var placa = "1234-ABC";
        var capacidadPaquetes = 10;

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            Vehiculo.Crear(tipo, placa, capacidadPaquetes));

        // Assert
        Assert.Equal("VEHICULO_TIPO_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Crear_con_placa_vacia_lanza_VEHICULO_PLACA_REQUERIDA()
    {
        // Arrange
        var tipo = "Motocicleta";
        var placa = string.Empty;
        var capacidadPaquetes = 10;

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            Vehiculo.Crear(tipo, placa, capacidadPaquetes));

        // Assert
        Assert.Equal("VEHICULO_PLACA_REQUERIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Crear_con_capacidad_menor_o_igual_a_cero_lanza_VEHICULO_CAPACIDAD_INVALIDA(int capacidadInvalida)
    {
        // Arrange
        var tipo = "Motocicleta";
        var placa = "1234-ABC";

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            Vehiculo.Crear(tipo, placa, capacidadInvalida)); // RN-27

        // Assert
        Assert.Equal("VEHICULO_CAPACIDAD_INVALIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Crear_con_datos_validos_construye_el_vehiculo()
    {
        // Arrange
        var tipo = "Motocicleta";
        var placa = "1234-ABC";
        var capacidadPaquetes = 10;

        // Act
        var vehiculo = Vehiculo.Crear(tipo, placa, capacidadPaquetes); // RN-27

        // Assert
        Assert.Equal(tipo, vehiculo.Tipo);
        Assert.Equal(placa, vehiculo.Placa);
        Assert.Equal(capacidadPaquetes, vehiculo.CapacidadPaquetes);
    }
}
