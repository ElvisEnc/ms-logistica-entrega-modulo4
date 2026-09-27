using Joseco.DDD.Core.Results;
using Logistica.Application.PaquetesRecibidos;
using Logistica.Application.Rutas.CrearRuta;
using Logistica.Application.Tests.TestSupport;

namespace Logistica.Application.Tests.Rutas.CrearRuta;

// §6.2 DESIGN.md: CrearRutaCommandHandler es donde viven las invariantes de orquestacion que
// RutaEntrega no puede garantizar por si sola porque cruzan agregados (I2, mitad de capacidad) o
// dependen de un repositorio (I9, I10, I11). Cada rama de error afirma ademas que no hubo
// efectos (UT-06): ni ruta agregada, ni repartidor mutado, ni paquetes marcados, ni commit.
[Trait("Capa", "Unit")]
public class CrearRutaCommandHandlerTests
{
    private static readonly DateOnly Fecha = PaqueteRecibidoBuilder.FechaPorDefecto;

    [Fact]
    public async Task Handle_repartidor_inexistente_lanza_REPARTIDOR_NO_ENCONTRADO()
    {
        // Arrange
        var repartidores = new RepartidorRepositoryFake();
        var rutas = new RutaEntregaRepositoryFake();
        var paquetesRecibidos = new PaqueteRecibidoStoreFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new CrearRutaCommandHandler(repartidores, rutas, paquetesRecibidos, unitOfWork);

        var command = new CrearRutaCommand(Fecha, Guid.NewGuid(), new[] { Guid.NewGuid() });

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None)); // busqueda de raiz -> 404

        // Assert
        Assert.Equal("REPARTIDOR_NO_ENCONTRADO", excepcion.Error.Code);
        Assert.Equal(ErrorType.NotFound, excepcion.Error.Type);
        Assert.Empty(rutas.RutasAgregadas); // sin efectos (UT-06)
        Assert.False(unitOfWork.Committed);
    }

    [Fact]
    public async Task Handle_repartidor_con_ruta_activa_lanza_I11_REPARTIDOR_CON_RUTA_ACTIVA()
    {
        // Arrange
        var repartidor = RepartidorBuilder.Disponible();
        var repartidores = new RepartidorRepositoryFake { TieneRutaActiva = true };
        repartidores.Agregar(repartidor);
        var rutas = new RutaEntregaRepositoryFake();
        var paquetesRecibidos = new PaqueteRecibidoStoreFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new CrearRutaCommandHandler(repartidores, rutas, paquetesRecibidos, unitOfWork);

        var command = new CrearRutaCommand(Fecha, repartidor.Id, new[] { Guid.NewGuid() });

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None)); // I11 (RN-27)

        // Assert
        Assert.Equal("I11_REPARTIDOR_CON_RUTA_ACTIVA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
        Assert.Empty(rutas.RutasAgregadas); // sin efectos (UT-06)
        Assert.False(unitOfWork.Committed);
        Assert.True(repartidor.EstaDisponible()); // AsignarRuta no llego a invocarse
    }

    [Fact]
    public async Task Handle_paquete_no_esta_POR_ASIGNAR_lanza_I9_PAQUETE_YA_ASIGNADO()
    {
        // Arrange
        var repartidor = RepartidorBuilder.Disponible();
        var repartidores = new RepartidorRepositoryFake();
        repartidores.Agregar(repartidor);
        var rutas = new RutaEntregaRepositoryFake();
        var paquetesRecibidos = new PaqueteRecibidoStoreFake(); // pool vacio: el paquete pedido no esta POR_ASIGNAR
        var unitOfWork = new UnitOfWorkFake();
        var handler = new CrearRutaCommandHandler(repartidores, rutas, paquetesRecibidos, unitOfWork);

        var command = new CrearRutaCommand(Fecha, repartidor.Id, new[] { Guid.NewGuid() });

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None)); // I9 (RN-17)

        // Assert
        Assert.Equal("I9_PAQUETE_YA_ASIGNADO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
        Assert.Empty(rutas.RutasAgregadas); // sin efectos (UT-06)
        Assert.False(unitOfWork.Committed);
        Assert.True(repartidor.EstaDisponible());
    }

    [Fact]
    public async Task Handle_paquete_con_fecha_distinta_lanza_I10_PAQUETES_FECHA_DISTINTA()
    {
        // Arrange
        var repartidor = RepartidorBuilder.Disponible();
        var repartidores = new RepartidorRepositoryFake();
        repartidores.Agregar(repartidor);
        var rutas = new RutaEntregaRepositoryFake();
        var paquete = PaqueteRecibidoBuilder.PorAsignar(fechaEntrega: Fecha.AddDays(1));
        var paquetesRecibidos = new PaqueteRecibidoStoreFake();
        paquetesRecibidos.Agregar(paquete);
        var unitOfWork = new UnitOfWorkFake();
        var handler = new CrearRutaCommandHandler(repartidores, rutas, paquetesRecibidos, unitOfWork);

        var command = new CrearRutaCommand(Fecha, repartidor.Id, new[] { paquete.PaqueteId });

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None)); // I10 (RN-10)

        // Assert
        Assert.Equal("I10_PAQUETES_FECHA_DISTINTA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
        Assert.Empty(rutas.RutasAgregadas); // sin efectos (UT-06)
        Assert.False(unitOfWork.Committed);
        Assert.True(repartidor.EstaDisponible());
    }

    [Fact]
    public async Task Handle_repartidor_sin_capacidad_lanza_I2_REPARTIDOR_NO_DISPONIBLE()
    {
        // Arrange
        var repartidor = RepartidorBuilder.Disponible(capacidadPaquetes: 1);
        var repartidores = new RepartidorRepositoryFake();
        repartidores.Agregar(repartidor);
        var rutas = new RutaEntregaRepositoryFake();
        var paquete1 = PaqueteRecibidoBuilder.PorAsignar(fechaEntrega: Fecha);
        var paquete2 = PaqueteRecibidoBuilder.PorAsignar(fechaEntrega: Fecha);
        var paquetesRecibidos = new PaqueteRecibidoStoreFake();
        paquetesRecibidos.Agregar(paquete1);
        paquetesRecibidos.Agregar(paquete2);
        var unitOfWork = new UnitOfWorkFake();
        var handler = new CrearRutaCommandHandler(repartidores, rutas, paquetesRecibidos, unitOfWork);

        var command = new CrearRutaCommand(Fecha, repartidor.Id, new[] { paquete1.PaqueteId, paquete2.PaqueteId });

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None)); // I2 (RN-27, RN-13) mitad de capacidad, Application

        // Assert
        Assert.Equal("I2_REPARTIDOR_NO_DISPONIBLE", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
        Assert.Empty(rutas.RutasAgregadas); // sin efectos (UT-06)
        Assert.False(unitOfWork.Committed);
        Assert.True(repartidor.EstaDisponible()); // AsignarRuta no llego a invocarse
        Assert.All(paquetesRecibidos.Paquetes, p => Assert.Equal(EstadoAsignacion.POR_ASIGNAR, p.Estado));
    }

    [Fact]
    public async Task Handle_caso_feliz_crea_la_ruta_asigna_al_repartidor_y_marca_los_paquetes()
    {
        // Arrange
        var repartidor = RepartidorBuilder.Disponible(capacidadPaquetes: 2);
        var repartidores = new RepartidorRepositoryFake();
        repartidores.Agregar(repartidor);
        var rutas = new RutaEntregaRepositoryFake();
        var paquete1 = PaqueteRecibidoBuilder.PorAsignar(fechaEntrega: Fecha);
        var paquete2 = PaqueteRecibidoBuilder.PorAsignar(fechaEntrega: Fecha);
        var paquetesRecibidos = new PaqueteRecibidoStoreFake();
        paquetesRecibidos.Agregar(paquete1);
        paquetesRecibidos.Agregar(paquete2);
        var unitOfWork = new UnitOfWorkFake();
        var handler = new CrearRutaCommandHandler(repartidores, rutas, paquetesRecibidos, unitOfWork);

        var command = new CrearRutaCommand(Fecha, repartidor.Id, new[] { paquete1.PaqueteId, paquete2.PaqueteId });

        // Act
        var rutaId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var rutaCreada = Assert.Single(rutas.RutasAgregadas); // AddAsync se llamo una sola vez
        Assert.Equal(rutaId, rutaCreada.RutaId.Value);
        Assert.Equal(repartidor.RepartidorId, rutaCreada.RepartidorId);
        Assert.Equal(2, rutaCreada.Paradas.Count);
        Assert.False(repartidor.EstaDisponible()); // I2 (RN-27) mitad de disponibilidad
        Assert.True(unitOfWork.Committed);
        Assert.All(paquetesRecibidos.Paquetes, p => Assert.Equal(EstadoAsignacion.ASIGNADO, p.Estado)); // I9 (RN-17)
    }
}
