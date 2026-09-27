using Joseco.DDD.Core.Results;
using Logistica.Application.PaquetesRecibidos;
using Logistica.Application.Tests.TestSupport;

namespace Logistica.Application.Tests.PaquetesRecibidos;

// §9.2/§9.4 DESIGN.md: unico handler de integracion entrante de BC6. No toca ningun repositorio
// de Domain ni hace commit por UnitOfWork; persiste directamente en el store del read model. I12
// (idempotencia) vive dentro de IPaqueteRecibidoStore.UpsertAsync — aqui se ejercita a traves del
// fake, que reproduce la semantica real (§9.3 DESIGN.md: ignora el duplicado, no sobrescribe).
[Trait("Capa", "Unit")]
public class ProcesarPaquetesListosCommandHandlerTests
{
    [Fact]
    public async Task Handle_paquete_valido_upsertea_un_PaqueteRecibido_mapeado_correctamente()
    {
        // Arrange
        var store = new PaqueteRecibidoStoreFake();
        var handler = new ProcesarPaquetesListosCommandHandler(store);
        var dto = PaqueteListoDtoBuilder.Valido();
        var contrato = PaqueteListoDtoBuilder.Lote(dto);
        var command = new ProcesarPaquetesListosCommand(contrato);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var recibido = Assert.Single(store.Paquetes);
        Assert.Equal(dto.PaqueteId, recibido.PaqueteId);
        Assert.Equal(dto.PacienteId, recibido.PacienteId);
        Assert.Equal(dto.PacienteNombre, recibido.PacienteNombre);
        Assert.Equal(dto.ContratoCateringId, recibido.ContratoCateringId);
        Assert.Equal(contrato.Fecha, recibido.FechaEntrega); // I10: fecha de la raiz del contrato, no del paquete
        Assert.Equal(EstadoAsignacion.POR_ASIGNAR, recibido.Estado);

        Assert.Equal(dto.DireccionEntrega.Calle, recibido.DireccionEntrega.Calle);
        Assert.Equal(dto.DireccionEntrega.Zona, recibido.DireccionEntrega.Zona);
        Assert.Equal(dto.DireccionEntrega.Ciudad, recibido.DireccionEntrega.Ciudad);
        Assert.Equal(dto.DireccionEntrega.Referencia, recibido.DireccionEntrega.Referencia);
        Assert.Equal(dto.DireccionEntrega.Coordenadas.Latitud, recibido.DireccionEntrega.Coordenadas.Latitud);
        Assert.Equal(dto.DireccionEntrega.Coordenadas.Longitud, recibido.DireccionEntrega.Coordenadas.Longitud);

        Assert.Equal(dto.Etiqueta.PaqueteId, recibido.Etiqueta.PaqueteId);
        Assert.Equal(dto.Etiqueta.NombrePaciente, recibido.Etiqueta.NombrePaciente);
        Assert.Equal(dto.Etiqueta.NroIdentificacion, recibido.Etiqueta.NroIdentificacion);
        Assert.Equal(dto.Etiqueta.Fecha, recibido.Etiqueta.Fecha);
        Assert.Equal(dto.Etiqueta.CodigoQR, recibido.Etiqueta.CodigoQR);
    }

    [Fact]
    public async Task Handle_reprocesar_el_mismo_paqueteId_no_duplica_I12()
    {
        // Arrange
        var store = new PaqueteRecibidoStoreFake();
        var paqueteId = Guid.NewGuid();
        // El paquete ya fue asignado a una ruta (I9): reprocesar el mismo evento de BC5 NO debe
        // devolverlo a POR_ASIGNAR, porque eso permitiria asignarlo dos veces (§9.3 DESIGN.md).
        var paqueteYaAsignado = PaqueteRecibidoBuilder.PorAsignar(paqueteId) with { Estado = EstadoAsignacion.ASIGNADO };
        store.Agregar(paqueteYaAsignado);
        var handler = new ProcesarPaquetesListosCommandHandler(store);
        var dto = PaqueteListoDtoBuilder.Valido(paqueteId: paqueteId); // mismo paqueteId, reenviado por BC5
        var contrato = PaqueteListoDtoBuilder.Lote(dto);
        var command = new ProcesarPaquetesListosCommand(contrato);

        // Act
        await handler.Handle(command, CancellationToken.None); // I12

        // Assert
        var recibido = Assert.Single(store.Paquetes); // no duplica
        Assert.Equal(EstadoAsignacion.ASIGNADO, recibido.Estado); // I12: no sobrescribe, no vuelve a POR_ASIGNAR
    }

    [Fact]
    public async Task Handle_paquete_con_PacienteId_vacio_lanza_PAQUETE_LISTO_PACIENTE_ID_REQUERIDO()
    {
        // Arrange
        var store = new PaqueteRecibidoStoreFake();
        var handler = new ProcesarPaquetesListosCommandHandler(store);
        var valido = PaqueteListoDtoBuilder.Valido();
        var invalido = PaqueteListoDtoBuilder.Valido(pacienteId: Guid.Empty);
        var contrato = PaqueteListoDtoBuilder.Lote(valido, invalido); // el invalido va segundo
        var command = new ProcesarPaquetesListosCommand(contrato);

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None)); // I14, defensa en profundidad

        // Assert
        Assert.Equal("PAQUETE_LISTO_PACIENTE_ID_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
        // El lote se aborta en el paquete invalido (no hay try/catch en el foreach): lo procesado
        // antes de llegar a el ya quedo persistido, y el invalido nunca se upsertea (UT-06).
        var recibido = Assert.Single(store.Paquetes);
        Assert.Equal(valido.PaqueteId, recibido.PaqueteId);
    }

    [Fact]
    public async Task Handle_paquete_con_ContratoCateringId_vacio_lanza_PAQUETE_LISTO_CONTRATO_CATERING_REQUERIDO()
    {
        // Arrange
        var store = new PaqueteRecibidoStoreFake();
        var handler = new ProcesarPaquetesListosCommandHandler(store);
        var valido = PaqueteListoDtoBuilder.Valido();
        var invalido = PaqueteListoDtoBuilder.Valido(contratoCateringId: Guid.Empty);
        var contrato = PaqueteListoDtoBuilder.Lote(valido, invalido); // el invalido va segundo
        var command = new ProcesarPaquetesListosCommand(contrato);

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(command, CancellationToken.None)); // I14, defensa en profundidad

        // Assert
        Assert.Equal("PAQUETE_LISTO_CONTRATO_CATERING_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
        // Mismo criterio que el caso de PacienteId: el lote se aborta en el paquete invalido,
        // sin deshacer lo que ya se proceso antes (UT-06).
        var recibido = Assert.Single(store.Paquetes);
        Assert.Equal(valido.PaqueteId, recibido.PaqueteId);
    }

    [Fact]
    public async Task Handle_procesa_todos_los_paquetes_del_lote()
    {
        // Arrange
        var store = new PaqueteRecibidoStoreFake();
        var handler = new ProcesarPaquetesListosCommandHandler(store);
        var paquete1 = PaqueteListoDtoBuilder.Valido();
        var paquete2 = PaqueteListoDtoBuilder.Valido();
        var paquete3 = PaqueteListoDtoBuilder.Valido();
        var contrato = PaqueteListoDtoBuilder.Lote(paquete1, paquete2, paquete3);
        var command = new ProcesarPaquetesListosCommand(contrato);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(3, store.Paquetes.Count);
        Assert.Contains(store.Paquetes, p => p.PaqueteId == paquete1.PaqueteId);
        Assert.Contains(store.Paquetes, p => p.PaqueteId == paquete2.PaqueteId);
        Assert.Contains(store.Paquetes, p => p.PaqueteId == paquete3.PaqueteId);
    }
}
