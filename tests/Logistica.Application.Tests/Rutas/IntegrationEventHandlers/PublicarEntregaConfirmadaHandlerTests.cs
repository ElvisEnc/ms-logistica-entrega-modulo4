using Logistica.Application.IntegrationEvents;
using Logistica.Application.Rutas.IntegrationEventHandlers;
using Logistica.Application.Tests.TestSupport;
using Logistica.Domain.Rutas.Events;
using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.Rutas.IntegrationEventHandlers;

// §8.3 DESIGN.md, SISTEMA.md §9.13: traductor de EntregaConfirmada (dominio) a
// EntregaConfirmadaIntegrationEvent (integracion). La constancia viaja ANIDADA (§9 del maestro,
// CLAUDE.md) y coordenadasConfirmacion existe en el payload aunque valga null.
[Trait("Capa", "Unit")]
public class PublicarEntregaConfirmadaHandlerTests
{
    [Fact]
    public async Task Handle_publica_EntregaConfirmadaIntegrationEvent_con_el_payload_de_SISTEMA_8_13()
    {
        // Arrange
        var rutaId = RutaId.New();
        var paradaId = ParadaId.New();
        var paqueteId = PaqueteId.New();
        var pacienteId = PacienteId.New();
        var fechaHora = new DateTime(2026, 3, 10, 14, 30, 0, DateTimeKind.Utc);
        var coordenadasConfirmacion = Coordenadas.Crear(-17.783206m, -63.182140m);
        var constancia = ConstanciaEntrega.Crear(fechaHora, TipoConstancia.FOTO, "https://evidencias/entrega1.jpg",
            "Juan Gomez", coordenadasConfirmacion);

        var notification = new EntregaConfirmada(rutaId, paradaId, paqueteId, pacienteId, "Maria Perez", constancia);

        var publisher = new IntegrationEventPublisherFake();
        var handler = new PublicarEntregaConfirmadaHandler(publisher);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var evento = Assert.IsType<EntregaConfirmadaIntegrationEvent>(publisher.Publicado);
        Assert.Equal(rutaId.Value, evento.RutaId);
        Assert.Equal(paradaId.Value, evento.ParadaId);
        Assert.Equal(paqueteId.Value, evento.PaqueteId);
        Assert.Equal(pacienteId.Value, evento.PacienteId); // I14: sin transformar
        Assert.Equal("Maria Perez", evento.PacienteNombre);
        Assert.Equal(TipoConstancia.FOTO, evento.Constancia.Tipo);
        Assert.Equal("https://evidencias/entrega1.jpg", evento.Constancia.UrlEvidencia);
        Assert.Equal("Juan Gomez", evento.Constancia.ReceptorNombre);
        Assert.Equal(fechaHora, evento.Constancia.FechaHora);
        Assert.NotNull(evento.Constancia.CoordenadasConfirmacion); // ANIDADA, no aplanada
        Assert.Equal(coordenadasConfirmacion.Latitud, evento.Constancia.CoordenadasConfirmacion!.Latitud);
        Assert.Equal(coordenadasConfirmacion.Longitud, evento.Constancia.CoordenadasConfirmacion!.Longitud);
    }

    [Fact]
    public async Task Handle_sin_coordenadas_de_confirmacion_publica_CoordenadasConfirmacion_null()
    {
        // Arrange
        var rutaId = RutaId.New();
        var paradaId = ParadaId.New();
        var paqueteId = PaqueteId.New();
        var pacienteId = PacienteId.New();
        var fechaHora = new DateTime(2026, 3, 10, 14, 30, 0, DateTimeKind.Utc);
        var constancia = ConstanciaEntrega.Crear(fechaHora, TipoConstancia.FIRMA, "https://evidencias/entrega2.jpg",
            "Juan Gomez", coordenadasConfirmacion: null);

        var notification = new EntregaConfirmada(rutaId, paradaId, paqueteId, pacienteId, "Maria Perez", constancia);

        var publisher = new IntegrationEventPublisherFake();
        var handler = new PublicarEntregaConfirmadaHandler(publisher);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var evento = Assert.IsType<EntregaConfirmadaIntegrationEvent>(publisher.Publicado);
        Assert.Null(evento.Constancia.CoordenadasConfirmacion); // campo presente, valor null (§9 del maestro)
    }
}
