using Joseco.DDD.Core.Results;
using Logistica.Application.Rutas.ConfirmarEntrega;
using Logistica.Application.Tests.TestSupport;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.Rutas.ConfirmarEntrega;

// MAPA §4.3 (2 filas) + §4.5 "Gap detectado" (ruta inexistente, extension no permitida, tamano
// excedido). La unica invariante de orquestacion propia de este handler es
// EVIDENCIA_ARCHIVO_REQUERIDO (RN-16): I4/I5 se prueban integros en Domain (§6.2). Los codigos
// EVIDENCIA_EXTENSION_NO_PERMITIDA y EVIDENCIA_TAMANO_EXCEDIDO son la resolucion CERRADA de
// docs/INCOHERENCIAS.md INC-1 (D-09 §19.2 Opcion A): MAPA-UNIT-TESTS.md §2 deja constancia de que
// no aparecen en la tabla de DESIGN.md §13.1/§13.2 porque esa tabla no se actualizo tras cerrar
// la incoherencia; el catalogo real, tal como lo declara RutaErrors, es el que se usa aqui.
[Trait("Capa", "Unit")]
public class ConfirmarEntregaHandlerTests
{
    private static Stream ArchivoValido() => new MemoryStream(new byte[] { 1, 2, 3 });

    [Fact]
    public async Task Handle_sin_archivo_lanza_EVIDENCIA_ARCHIVO_REQUERIDO()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var rutas = new RutaEntregaRepositoryFake();
        await rutas.AddAsync(ruta);
        var evidencias = new EvidenciaStorageFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new ConfirmarEntregaCommandHandler(rutas, evidencias, unitOfWork);

        var parada = ruta.Paradas.Single();
        var command = new ConfirmarEntregaCommand(
            ruta.RutaId.Value, parada.ParadaId.Value, TipoConstancia.FOTO, null, null, "Juan Perez", null, null);

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None)); // RN-16

        // Assert
        Assert.Equal("EVIDENCIA_ARCHIVO_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
        Assert.False(evidencias.Llamado); // sin efectos (UT-06)
        Assert.False(unitOfWork.Committed);
        Assert.Equal(EstadoEntrega.EN_CAMINO, parada.Estado);
    }

    [Fact]
    public async Task Handle_con_archivo_guarda_la_evidencia_y_la_url_queda_en_la_constancia()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var rutas = new RutaEntregaRepositoryFake();
        await rutas.AddAsync(ruta);
        var evidencias = new EvidenciaStorageFake { UrlAGuardar = "https://storage.local/evidencias/foto-parada.jpg" };
        var unitOfWork = new UnitOfWorkFake();
        var handler = new ConfirmarEntregaCommandHandler(rutas, evidencias, unitOfWork);

        var parada = ruta.Paradas.Single();
        var command = new ConfirmarEntregaCommand(
            ruta.RutaId.Value, parada.ParadaId.Value, TipoConstancia.FOTO, ArchivoValido(), "foto.jpg", "Juan Perez", null, null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(EstadoEntrega.ENTREGADO, parada.Estado);
        Assert.NotNull(parada.Constancia);
        Assert.Equal("https://storage.local/evidencias/foto-parada.jpg", parada.Constancia!.UrlEvidencia);
        Assert.Null(parada.Incidencia); // I5 (RN-16): la mitad que hace de I5 una garantia
        Assert.True(evidencias.Llamado);
        Assert.True(unitOfWork.Committed);
    }

    [Fact]
    public async Task Handle_ruta_inexistente_lanza_RUTA_NO_ENCONTRADA()
    {
        // Arrange
        var rutas = new RutaEntregaRepositoryFake();
        var evidencias = new EvidenciaStorageFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new ConfirmarEntregaCommandHandler(rutas, evidencias, unitOfWork);

        var command = new ConfirmarEntregaCommand(
            Guid.NewGuid(), Guid.NewGuid(), TipoConstancia.FOTO, ArchivoValido(), "foto.jpg", "Juan Perez", null, null);

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None)); // busqueda de raiz -> 404

        // Assert
        Assert.Equal("RUTA_NO_ENCONTRADA", excepcion.Error.Code);
        Assert.Equal(ErrorType.NotFound, excepcion.Error.Type);
        Assert.False(evidencias.Llamado); // sin efectos (UT-06)
        Assert.False(unitOfWork.Committed);
    }

    [Fact]
    public async Task Handle_extension_no_permitida_lanza_EVIDENCIA_EXTENSION_NO_PERMITIDA()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var rutas = new RutaEntregaRepositoryFake();
        await rutas.AddAsync(ruta);
        var evidencias = new EvidenciaStorageFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new ConfirmarEntregaCommandHandler(rutas, evidencias, unitOfWork);

        var parada = ruta.Paradas.Single();
        var command = new ConfirmarEntregaCommand(
            ruta.RutaId.Value, parada.ParadaId.Value, TipoConstancia.FOTO, ArchivoValido(), "evidencia.pdf", "Juan Perez", null, null);

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None)); // D-09, INC-1 Opcion A (§19.2 DESIGN.md)

        // Assert
        Assert.Equal("EVIDENCIA_EXTENSION_NO_PERMITIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
        Assert.False(evidencias.Llamado); // sin efectos (UT-06)
        Assert.False(unitOfWork.Committed);
        Assert.Equal(EstadoEntrega.EN_CAMINO, parada.Estado);
    }

    [Fact]
    public async Task Handle_archivo_excede_el_tamano_maximo_lanza_EVIDENCIA_TAMANO_EXCEDIDO()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var rutas = new RutaEntregaRepositoryFake();
        await rutas.AddAsync(ruta);
        var evidencias = new EvidenciaStorageFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new ConfirmarEntregaCommandHandler(rutas, evidencias, unitOfWork);

        var parada = ruta.Paradas.Single();
        var archivoGrande = new MemoryStream(new byte[6 * 1024 * 1024]); // > 5 MB (D-09, INC-1 Opcion A)
        var command = new ConfirmarEntregaCommand(
            ruta.RutaId.Value, parada.ParadaId.Value, TipoConstancia.FOTO, archivoGrande, "foto.jpg", "Juan Perez", null, null);

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("EVIDENCIA_TAMANO_EXCEDIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
        Assert.False(evidencias.Llamado); // sin efectos (UT-06)
        Assert.False(unitOfWork.Committed);
        Assert.Equal(EstadoEntrega.EN_CAMINO, parada.Estado);
    }
}
