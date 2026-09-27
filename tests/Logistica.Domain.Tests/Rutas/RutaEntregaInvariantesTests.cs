using Joseco.DDD.Core.Results;
using Logistica.Domain.Rutas;
using Logistica.Domain.Rutas.Events;
using Logistica.Domain.Shared;
using Logistica.Domain.Tests.TestSupport;

namespace Logistica.Domain.Tests.Rutas;

// Cubre §4.1 del MAPA de unit tests para I1, I3, I6, I7, I13 (RutaEntrega, raiz del agregado).
// I2 (disponibilidad) vive en RepartidorTests; I4/I5 en ParadaEntregaTests; I8/optimizacion en
// RutaEntregaOptimizacionTests (fuera del alcance de esta suite).
[Trait("Capa", "Unit")]
public class RutaEntregaInvariantesTests
{
    [Fact]
    public void Crear_con_lista_vacia_lanza_I1_RUTA_SIN_PARADAS()
    {
        // Arrange
        var paquetes = Array.Empty<PaqueteParaRuta>();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            RutaEntrega.Crear(RutaEntregaBuilder.FechaPorDefecto, RepartidorId.New(), paquetes)); // I1 (RN-13)

        // Assert
        Assert.Equal("I1_RUTA_SIN_PARADAS", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Crear_con_paquetes_nace_pendiente_con_una_parada_por_paquete()
    {
        // Arrange
        var paquetes = new[] { RutaEntregaBuilder.Paquete(), RutaEntregaBuilder.Paquete(), RutaEntregaBuilder.Paquete() };

        // Act
        var ruta = RutaEntrega.Crear(RutaEntregaBuilder.FechaPorDefecto, RepartidorId.New(), paquetes); // I1 (RN-13)

        // Assert
        Assert.Equal(EstadoRuta.PENDIENTE, ruta.Estado);
        Assert.Equal(paquetes.Length, ruta.Paradas.Count);
        Assert.Equal(
            paquetes.Select(p => p.PaqueteId).OrderBy(id => id.Value).ToList(),
            ruta.Paradas.Select(p => p.PaqueteId).OrderBy(id => id.Value).ToList());
    }

    [Fact]
    public void Optimizar_sobre_ruta_no_pendiente_lanza_I3_RUTA_NO_PENDIENTE()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            ruta.Optimizar(RutaEntregaBuilder.Origen())); // I3 (RN-13, RN-15)

        // Assert
        Assert.Equal("I3_RUTA_NO_PENDIENTE", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
    }

    [Fact]
    public void Iniciar_sobre_ruta_no_optimizada_lanza_I3_RUTA_NO_OPTIMIZADA()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.Pendiente();

        // Act
        var excepcion = Assert.Throws<DomainException>(ruta.Iniciar); // I3 (RN-13, RN-15)

        // Assert
        Assert.Equal("I3_RUTA_NO_OPTIMIZADA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
    }

    [Fact]
    public void Iniciar_sobre_ruta_no_pendiente_lanza_I3_RUTA_NO_PENDIENTE()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();

        // Act
        var excepcion = Assert.Throws<DomainException>(ruta.Iniciar); // I3 (RN-13, RN-15)

        // Assert
        Assert.Equal("I3_RUTA_NO_PENDIENTE", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
    }

    [Fact]
    public void Completar_con_paradas_pendientes_lanza_I6_PARADAS_PENDIENTES()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();

        // Act
        var excepcion = Assert.Throws<DomainException>(ruta.Completar); // I6 (RN-16)

        // Assert
        Assert.Equal("I6_PARADAS_PENDIENTES", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
    }

    [Fact]
    public void Completar_con_todas_ENTREGADO_da_COMPLETADA_I7()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCaminoConTodasEntregadas();

        // Act
        ruta.Completar(); // I7 (RN-16)

        // Assert
        Assert.Equal(EstadoRuta.COMPLETADA, ruta.Estado);
    }

    [Fact]
    public void Completar_con_una_NO_ENTREGADO_da_CON_INCIDENCIAS_I7()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCaminoConUnaIncidencia();

        // Act
        ruta.Completar(); // I7 (RN-16)

        // Assert
        Assert.Equal(EstadoRuta.CON_INCIDENCIAS, ruta.Estado);
    }

    [Fact]
    public void Cancelar_con_motivo_vacio_lanza_I13_MOTIVO_REQUERIDO()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.Pendiente();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            ruta.Cancelar(string.Empty)); // I13 (RN-13)

        // Assert
        Assert.Equal("I13_MOTIVO_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Cancelar_ruta_ya_CANCELADA_lanza_I13_RUTA_NO_CANCELABLE()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.Pendiente();
        ruta.Cancelar("El paciente ya no requiere la entrega");

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            ruta.Cancelar("Segundo intento de cancelacion")); // I13 (RN-13)

        // Assert
        Assert.Equal("I13_RUTA_NO_CANCELABLE", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
    }

    [Fact]
    public void Cancelar_ruta_COMPLETADA_lanza_I13_RUTA_NO_CANCELABLE()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCaminoConTodasEntregadas();
        ruta.Completar();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            ruta.Cancelar("La ruta ya se completo")); // I13 (RN-13)

        // Assert
        Assert.Equal("I13_RUTA_NO_CANCELABLE", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
    }

    [Fact]
    public void Cancelar_desde_pendiente_pasa_a_CANCELADA_y_emite_RutaCancelada_con_todos_los_paquetes()
    {
        // Arrange
        var repartidorId = RepartidorId.New();
        var ruta = RutaEntregaBuilder.Pendiente(cantidadParadas: 2, repartidorId: repartidorId);
        var paqueteIdsEsperados = ruta.Paradas.Select(p => p.PaqueteId).OrderBy(id => id.Value).ToList();

        // Act
        ruta.Cancelar("El paciente ya no requiere la entrega"); // I13 (RN-13)

        // Assert
        Assert.Equal(EstadoRuta.CANCELADA, ruta.Estado);

        var evento = Assert.Single(ruta.DomainEvents.OfType<RutaCancelada>());
        Assert.Equal(ruta.RutaId, evento.RutaId);
        Assert.Equal(repartidorId, evento.RepartidorId);
        Assert.Equal("El paciente ya no requiere la entrega", evento.Motivo);
        Assert.Equal(paqueteIdsEsperados, evento.PaqueteIdsNoResueltos.OrderBy(id => id.Value).ToList());
    }

    [Fact]
    public void ReportarIncidencia_conserva_PacienteId_y_ContratoCateringId_en_el_evento_I14()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var parada = ruta.Paradas.Single();
        var incidencia = RutaEntregaBuilder.IncidenciaValida();

        // Act
        ruta.ReportarIncidencia(parada.ParadaId, incidencia); // I14 (RN-16, RN-23)

        // Assert
        var evento = Assert.Single(ruta.DomainEvents.OfType<IncidenciaEntregaRegistrada>());
        Assert.Equal(parada.PaqueteId, evento.PaqueteId);
        Assert.Equal(parada.PacienteId, evento.PacienteId);
        Assert.Equal(parada.ContratoCateringId, evento.ContratoCateringId);
    }
}
