using Joseco.DDD.Core.Results;
using Logistica.Application.Rutas.ReportarIncidencia;
using Logistica.Application.Tests.TestSupport;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.Rutas.ReportarIncidencia;

// MAPA §4.5 "Gap detectado" (5 filas): ReportarIncidenciaCommandHandler no tenia ninguna fila en
// §4.3 (§10.1 DESIGN.md: ninguna invariante de orquestacion propia, el archivo es OPCIONAL aqui a
// diferencia de ConfirmarEntrega, decision de negocio y no una omision). I4, I5 e I14 se prueban
// integros en Domain (§6.2). Los codigos EVIDENCIA_EXTENSION_NO_PERMITIDA y
// EVIDENCIA_TAMANO_EXCEDIDO son los mismos objetos RutaErrors compartidos con
// ConfirmarEntregaCommandHandler via EvidenciaArchivoValidator (D-09 §19.2 Opcion A): el guard solo
// se ejecuta si llega archivo (a diferencia de ConfirmarEntrega, donde el archivo es obligatorio).
[Trait("Capa", "Unit")]
public class ReportarIncidenciaHandlerTests
{
    private static Stream ArchivoValido() => new MemoryStream(new byte[] { 1, 2, 3 });

    [Fact]
    public async Task Handle_ruta_inexistente_lanza_RUTA_NO_ENCONTRADA()
    {
        // Arrange
        var rutas = new RutaEntregaRepositoryFake();
        var evidencias = new EvidenciaStorageFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new ReportarIncidenciaCommandHandler(rutas, evidencias, unitOfWork);

        var command = new ReportarIncidenciaCommand(
            Guid.NewGuid(), Guid.NewGuid(), MotivoIncidencia.PACIENTE_AUSENTE, "No abrio la puerta", null, null);

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
        var handler = new ReportarIncidenciaCommandHandler(rutas, evidencias, unitOfWork);

        var parada = ruta.Paradas.Single();
        var command = new ReportarIncidenciaCommand(
            ruta.RutaId.Value, parada.ParadaId.Value, MotivoIncidencia.PAQUETE_DANADO, "Paquete roto",
            ArchivoValido(), "evidencia.pdf");

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
        var handler = new ReportarIncidenciaCommandHandler(rutas, evidencias, unitOfWork);

        var parada = ruta.Paradas.Single();
        var archivoGrande = new MemoryStream(new byte[6 * 1024 * 1024]); // > 5 MB (D-09, INC-1 Opcion A)
        var command = new ReportarIncidenciaCommand(
            ruta.RutaId.Value, parada.ParadaId.Value, MotivoIncidencia.DIRECCION_NO_ENCONTRADA,
            "No se hallo la direccion", archivoGrande, "foto.jpg");

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

    [Fact]
    public async Task Handle_con_foto_guarda_la_evidencia_y_la_url_queda_en_la_incidencia()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var rutas = new RutaEntregaRepositoryFake();
        await rutas.AddAsync(ruta);
        var evidencias = new EvidenciaStorageFake { UrlAGuardar = "https://storage.local/evidencias/incidencia-parada.jpg" };
        var unitOfWork = new UnitOfWorkFake();
        var handler = new ReportarIncidenciaCommandHandler(rutas, evidencias, unitOfWork);

        var parada = ruta.Paradas.Single();
        var command = new ReportarIncidenciaCommand(
            ruta.RutaId.Value, parada.ParadaId.Value, MotivoIncidencia.RECHAZADO_POR_PACIENTE,
            "El paciente rechazo el paquete", ArchivoValido(), "foto.jpg");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(EstadoEntrega.NO_ENTREGADO, parada.Estado);
        Assert.NotNull(parada.Incidencia);
        Assert.Equal("https://storage.local/evidencias/incidencia-parada.jpg", parada.Incidencia!.UrlFoto);
        Assert.Null(parada.Constancia); // I5 (RN-16): la mitad que hace de I5 una garantia
        Assert.True(evidencias.Llamado);
        Assert.True(unitOfWork.Committed);
    }

    [Fact]
    public async Task Handle_sin_foto_registra_la_incidencia_con_UrlFoto_null()
    {
        // Arrange: el archivo es OPCIONAL aqui (§10.1, §18.3 DESIGN.md) -- decision de negocio, no
        // una omision, a diferencia de ConfirmarEntrega donde es obligatorio (EVIDENCIA_ARCHIVO_REQUERIDO).
        var ruta = RutaEntregaBuilder.EnCamino();
        var rutas = new RutaEntregaRepositoryFake();
        await rutas.AddAsync(ruta);
        var evidencias = new EvidenciaStorageFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new ReportarIncidenciaCommandHandler(rutas, evidencias, unitOfWork);

        var parada = ruta.Paradas.Single();
        var command = new ReportarIncidenciaCommand(
            ruta.RutaId.Value, parada.ParadaId.Value, MotivoIncidencia.OTRO, "Motivo no catalogado", null, null);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(EstadoEntrega.NO_ENTREGADO, parada.Estado);
        Assert.NotNull(parada.Incidencia);
        Assert.Null(parada.Incidencia!.UrlFoto);
        Assert.Null(parada.Constancia); // I5 (RN-16): simetrico de ConfirmarEntrega
        Assert.False(evidencias.Llamado); // sin archivo, el storage nunca se invoca
        Assert.True(unitOfWork.Committed);
    }
}
