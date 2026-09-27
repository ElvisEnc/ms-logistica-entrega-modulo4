using Joseco.DDD.Core.Results;
using Logistica.Domain.Repartidores;
using Logistica.Domain.Shared;
using Logistica.Domain.Tests.TestSupport;

namespace Logistica.Domain.Tests.Repartidores;

[Trait("Capa", "Unit")]
public class RepartidorTests
{
    [Fact]
    public void AsignarRuta_sobre_no_disponible_lanza_I2_REPARTIDOR_NO_DISPONIBLE()
    {
        // Arrange
        var repartidor = Repartidor.Registrar("Juan Pérez", "70000000", VehiculoBuilder.Valido());
        repartidor.AsignarRuta();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            repartidor.AsignarRuta()); // I2 (RN-27, RN-13)

        // Assert
        Assert.Equal("I2_REPARTIDOR_NO_DISPONIBLE", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
    }

    [Fact]
    public void TieneCapacidadPara_cantidad_igual_a_la_capacidad_es_true()
    {
        // Arrange
        var repartidor = Repartidor.Registrar("Juan Pérez", "70000000", VehiculoBuilder.Valido(10));

        // Act
        var tieneCapacidad = repartidor.TieneCapacidadPara(10); // I2 (RN-27, RN-13)

        // Assert
        Assert.True(tieneCapacidad);
    }

    [Fact]
    public void TieneCapacidadPara_cantidad_mayor_a_la_capacidad_es_false()
    {
        // Arrange
        var repartidor = Repartidor.Registrar("Juan Pérez", "70000000", VehiculoBuilder.Valido(10));

        // Act
        var tieneCapacidad = repartidor.TieneCapacidadPara(11); // I2 (RN-27, RN-13)

        // Assert
        Assert.False(tieneCapacidad);
    }

    [Fact]
    public void Liberar_es_idempotente_y_no_lanza_si_ya_estaba_disponible()
    {
        // Arrange
        var repartidor = Repartidor.Registrar("Juan Pérez", "70000000", VehiculoBuilder.Valido());

        // Act
        var excepcion = Record.Exception(() => repartidor.Liberar());

        // Assert
        Assert.Null(excepcion);
        Assert.True(repartidor.EstaDisponible());
    }

    [Fact]
    public void Registrar_con_nombre_vacio_lanza_REPARTIDOR_NOMBRE_REQUERIDO()
    {
        // Arrange
        var nombre = string.Empty;
        var telefono = "70000000";
        var vehiculo = VehiculoBuilder.Valido();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            Repartidor.Registrar(nombre, telefono, vehiculo));

        // Assert
        Assert.Equal("REPARTIDOR_NOMBRE_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Registrar_con_telefono_vacio_lanza_REPARTIDOR_TELEFONO_REQUERIDO()
    {
        // Arrange
        var nombre = "Juan Pérez";
        var telefono = string.Empty;
        var vehiculo = VehiculoBuilder.Valido();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            Repartidor.Registrar(nombre, telefono, vehiculo));

        // Assert
        Assert.Equal("REPARTIDOR_TELEFONO_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Registrar_con_vehiculo_nulo_lanza_REPARTIDOR_VEHICULO_REQUERIDO()
    {
        // Arrange
        var nombre = "Juan Pérez";
        var telefono = "70000000";
        Vehiculo vehiculo = null!;

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            Repartidor.Registrar(nombre, telefono, vehiculo)); // RN-27

        // Assert
        Assert.Equal("REPARTIDOR_VEHICULO_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Registrar_con_datos_validos_crea_el_repartidor_disponible()
    {
        // Arrange
        var nombre = "Juan Pérez";
        var telefono = "70000000";
        var vehiculo = VehiculoBuilder.Valido();

        // Act
        var repartidor = Repartidor.Registrar(nombre, telefono, vehiculo);

        // Assert
        Assert.Equal(nombre, repartidor.Nombre);
        Assert.Equal(telefono, repartidor.Telefono);
        Assert.Equal(vehiculo, repartidor.Vehiculo);
        Assert.True(repartidor.EstaDisponible());
    }

    [Fact]
    public void AsignarRuta_sobre_disponible_lo_marca_no_disponible()
    {
        // Arrange
        var repartidor = Repartidor.Registrar("Juan Pérez", "70000000", VehiculoBuilder.Valido());

        // Act
        repartidor.AsignarRuta();

        // Assert
        Assert.False(repartidor.EstaDisponible());
    }

    [Fact]
    public void Liberar_sobre_no_disponible_lo_marca_disponible()
    {
        // Arrange
        var repartidor = Repartidor.Registrar("Juan Pérez", "70000000", VehiculoBuilder.Valido());
        repartidor.AsignarRuta();

        // Act
        repartidor.Liberar();

        // Assert
        Assert.True(repartidor.EstaDisponible());
    }
}
