using Logistica.Application.IntegrationEvents;
using Logistica.Application.Rutas.IntegrationEventHandlers;
using Logistica.Application.Tests.TestSupport;
using Logistica.Domain.Rutas.Events;
using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.Rutas.IntegrationEventHandlers;

// §8.3 DESIGN.md, SISTEMA.md §9.14: traductor de IncidenciaEntregaRegistrada (dominio) a
// IncidenciaEntregaRegistradaIntegrationEvent (integracion). Diez campos, no nueve: urlFoto
// existe siempre en el payload aunque su valor sea null (CLAUDE.md, SISTEMA.md §9.14).
[Trait("Capa", "Unit")]
public class PublicarIncidenciaEntregaRegistradaHandlerTests
{
    [Fact]
    public async Task Handle_publica_IncidenciaEntregaRegistradaIntegrationEvent_con_el_payload_de_SISTEMA_8_14()
    {
        // Arrange
        var rutaId = RutaId.New();
        var paradaId = ParadaId.New();
        var paqueteId = PaqueteId.New();
        var pacienteId = PacienteId.New();
        var contratoCateringId = ContratoId.New();
        var fechaHora = new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc);
        var incidencia = IncidenciaEntrega.Crear(fechaHora, MotivoIncidencia.PACIENTE_AUSENTE,
            "El paciente no se encontraba en el domicilio", "https://evidencias/incidencia1.jpg");

        var notification = new IncidenciaEntregaRegistrada(
            rutaId, paradaId, paqueteId, pacienteId, "Maria Perez", contratoCateringId, incidencia);

        var publisher = new IntegrationEventPublisherFake();
        var handler = new PublicarIncidenciaEntregaRegistradaHandler(publisher);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var evento = Assert.IsType<IncidenciaEntregaRegistradaIntegrationEvent>(publisher.Publicado);
        Assert.Equal(rutaId.Value, evento.RutaId);
        Assert.Equal(paradaId.Value, evento.ParadaId);
        Assert.Equal(paqueteId.Value, evento.PaqueteId);
        Assert.Equal(pacienteId.Value, evento.PacienteId); // I14: sin transformar
        Assert.Equal("Maria Perez", evento.PacienteNombre);
        Assert.Equal(contratoCateringId.Value, evento.ContratoCateringId); // I14: sin transformar
        Assert.Equal(MotivoIncidencia.PACIENTE_AUSENTE, evento.Motivo);
        Assert.Equal("El paciente no se encontraba en el domicilio", evento.Descripcion);
        Assert.Equal(fechaHora, evento.FechaHora);
        Assert.Equal("https://evidencias/incidencia1.jpg", evento.UrlFoto); // decimo campo
    }

    [Fact]
    public async Task Handle_sin_foto_publica_UrlFoto_null()
    {
        // Arrange
        var rutaId = RutaId.New();
        var paradaId = ParadaId.New();
        var paqueteId = PaqueteId.New();
        var pacienteId = PacienteId.New();
        var contratoCateringId = ContratoId.New();
        var fechaHora = new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc);
        var incidencia = IncidenciaEntrega.Crear(fechaHora, MotivoIncidencia.DIRECCION_NO_ENCONTRADA,
            "No se encontro la direccion registrada", urlFoto: null);

        var notification = new IncidenciaEntregaRegistrada(
            rutaId, paradaId, paqueteId, pacienteId, "Maria Perez", contratoCateringId, incidencia);

        var publisher = new IntegrationEventPublisherFake();
        var handler = new PublicarIncidenciaEntregaRegistradaHandler(publisher);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var evento = Assert.IsType<IncidenciaEntregaRegistradaIntegrationEvent>(publisher.Publicado);
        Assert.Null(evento.UrlFoto); // campo presente, valor null (§9.14 del maestro)
    }
}
