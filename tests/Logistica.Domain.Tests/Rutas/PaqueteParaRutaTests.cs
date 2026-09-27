using Joseco.DDD.Core.Results;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;
using Logistica.Domain.Tests.TestSupport;

namespace Logistica.Domain.Tests.Rutas;

[Trait("Capa", "Unit")]
public class PaqueteParaRutaTests
{
    [Fact]
    public void PacienteId_nulo_lanza_PAQUETE_RUTA_PACIENTE_ID_REQUERIDO()
    {
        // Arrange
        var paqueteId = PaqueteId.New();
        PacienteId pacienteId = null!;
        var direccionEntrega = DireccionGeoBuilder.Valida();
        var contratoCateringId = ContratoId.New();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            PaqueteParaRuta.Crear(paqueteId, pacienteId, "Juan Pérez", direccionEntrega, contratoCateringId)); // I14 (RN-16, RN-23)

        // Assert
        Assert.Equal("PAQUETE_RUTA_PACIENTE_ID_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void ContratoCateringId_nulo_lanza_PAQUETE_RUTA_CONTRATO_CATERING_REQUERIDO()
    {
        // Arrange
        var paqueteId = PaqueteId.New();
        var pacienteId = PacienteId.New();
        var direccionEntrega = DireccionGeoBuilder.Valida();
        ContratoId contratoCateringId = null!;

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            PaqueteParaRuta.Crear(paqueteId, pacienteId, "Juan Pérez", direccionEntrega, contratoCateringId)); // I14 (RN-16, RN-23)

        // Assert
        Assert.Equal("PAQUETE_RUTA_CONTRATO_CATERING_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void PaqueteId_nulo_lanza_PAQUETE_RUTA_PAQUETE_ID_REQUERIDO()
    {
        // Arrange
        PaqueteId paqueteId = null!;
        var pacienteId = PacienteId.New();
        var direccionEntrega = DireccionGeoBuilder.Valida();
        var contratoCateringId = ContratoId.New();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            PaqueteParaRuta.Crear(paqueteId, pacienteId, "Juan Pérez", direccionEntrega, contratoCateringId));

        // Assert
        Assert.Equal("PAQUETE_RUTA_PAQUETE_ID_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void PacienteNombre_vacio_lanza_PAQUETE_RUTA_PACIENTE_NOMBRE_REQUERIDO()
    {
        // Arrange
        var paqueteId = PaqueteId.New();
        var pacienteId = PacienteId.New();
        var direccionEntrega = DireccionGeoBuilder.Valida();
        var contratoCateringId = ContratoId.New();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            PaqueteParaRuta.Crear(paqueteId, pacienteId, string.Empty, direccionEntrega, contratoCateringId));

        // Assert
        Assert.Equal("PAQUETE_RUTA_PACIENTE_NOMBRE_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void DireccionEntrega_nula_lanza_PAQUETE_RUTA_DIRECCION_REQUERIDA()
    {
        // Arrange
        var paqueteId = PaqueteId.New();
        var pacienteId = PacienteId.New();
        DireccionGeo direccionEntrega = null!;
        var contratoCateringId = ContratoId.New();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            PaqueteParaRuta.Crear(paqueteId, pacienteId, "Juan Pérez", direccionEntrega, contratoCateringId));

        // Assert
        Assert.Equal("PAQUETE_RUTA_DIRECCION_REQUERIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Crear_con_todos_los_campos_validos_construye_el_paquete()
    {
        // Arrange
        var paqueteId = PaqueteId.New();
        var pacienteId = PacienteId.New();
        var direccionEntrega = DireccionGeoBuilder.Valida();
        var contratoCateringId = ContratoId.New();

        // Act
        var paquete = PaqueteParaRuta.Crear(paqueteId, pacienteId, "Juan Pérez", direccionEntrega, contratoCateringId);

        // Assert
        Assert.Equal(paqueteId, paquete.PaqueteId);
        Assert.Equal(pacienteId, paquete.PacienteId);
        Assert.Equal("Juan Pérez", paquete.PacienteNombre);
        Assert.Equal(direccionEntrega, paquete.DireccionEntrega);
        Assert.Equal(contratoCateringId, paquete.ContratoCateringId);
    }
}
