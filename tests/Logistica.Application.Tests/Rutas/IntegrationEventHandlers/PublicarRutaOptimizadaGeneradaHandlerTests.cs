using Logistica.Application.IntegrationEvents;
using Logistica.Application.Rutas.IntegrationEventHandlers;
using Logistica.Application.Tests.TestSupport;
using Logistica.Domain.Rutas.Events;
using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.Rutas.IntegrationEventHandlers;

// §8.3 DESIGN.md, SISTEMA.md §9.12: traductor de RutaOptimizadaGenerada (dominio) a
// RutaOptimizadaGeneradaIntegrationEvent (integracion), via DomainToIntegrationMapper.
[Trait("Capa", "Unit")]
public class PublicarRutaOptimizadaGeneradaHandlerTests
{
    [Fact]
    public async Task Handle_publica_RutaOptimizadaGeneradaIntegrationEvent_con_el_payload_de_SISTEMA_8_12()
    {
        // Arrange
        var rutaId = RutaId.New();
        var repartidorId = RepartidorId.New();
        var paradaId = ParadaId.New();
        var paqueteId = PaqueteId.New();
        var pacienteId = PacienteId.New();
        var fecha = new DateOnly(2026, 3, 10);
        var direccion = DireccionGeoBuilder.Valida();

        var parada = new ParadaOrdenada(paradaId, 1, paqueteId, pacienteId, "Maria Perez", direccion);
        var notification = new RutaOptimizadaGenerada(rutaId, repartidorId, fecha, [parada]);

        var publisher = new IntegrationEventPublisherFake();
        var handler = new PublicarRutaOptimizadaGeneradaHandler(publisher);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        var evento = Assert.IsType<RutaOptimizadaGeneradaIntegrationEvent>(publisher.Publicado);
        Assert.Equal(rutaId.Value, evento.RutaId);
        Assert.Equal(repartidorId.Value, evento.RepartidorId);
        Assert.Equal(fecha, evento.Fecha);
        var paradaDto = Assert.Single(evento.Paradas);
        Assert.Equal(paradaId.Value, paradaDto.ParadaId);
        Assert.Equal(1, paradaDto.Orden);
        Assert.Equal(paqueteId.Value, paradaDto.PaqueteId);
        Assert.Equal(pacienteId.Value, paradaDto.PacienteId);
        Assert.Equal("Maria Perez", paradaDto.PacienteNombre);
        Assert.Equal(direccion.Calle, paradaDto.DireccionEntrega.Calle);
        Assert.Equal(direccion.Zona, paradaDto.DireccionEntrega.Zona);
        Assert.Equal(direccion.Ciudad, paradaDto.DireccionEntrega.Ciudad);
        Assert.Equal(direccion.Referencia, paradaDto.DireccionEntrega.Referencia);
        Assert.Equal(direccion.Coordenadas.Latitud, paradaDto.DireccionEntrega.Coordenadas.Latitud);
        Assert.Equal(direccion.Coordenadas.Longitud, paradaDto.DireccionEntrega.Coordenadas.Longitud);
    }
}
