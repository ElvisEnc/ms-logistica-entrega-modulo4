using Logistica.Application.PaquetesRecibidos;
using Logistica.Application.Rutas.Policies;
using Logistica.Application.Tests.TestSupport;
using Logistica.Domain.Rutas.Events;
using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.Rutas.Policies;

// §8.2 DESIGN.md: reposicion de I9 (RN-17) al cancelar una ruta. Lee los PaqueteId DEL EVENTO
// (PaqueteIdsNoResueltos), no vuelve a cargar el agregado, y escribe sobre IPaqueteRecibidoStore.
[Trait("Capa", "Unit")]
public class ReponerPaquetesAlCancelarRutaPolicyTests
{
    [Fact]
    public async Task Handle_marca_por_asignar_los_paquetes_no_resueltos()
    {
        // Arrange
        var paquete = PaqueteRecibidoBuilder.PorAsignar() with { Estado = EstadoAsignacion.ASIGNADO };
        var paquetesRecibidos = new PaqueteRecibidoStoreFake();
        paquetesRecibidos.Agregar(paquete);

        var policy = new ReponerPaquetesAlCancelarRutaPolicy(paquetesRecibidos);
        var notification = new RutaCancelada(
            RutaId.New(),
            RepartidorId.New(),
            "Vehiculo averiado",
            new[] { PaqueteId.From(paquete.PaqueteId) });

        // Act
        await policy.Handle(notification, CancellationToken.None);

        // Assert
        var paqueteActualizado = Assert.Single(paquetesRecibidos.Paquetes);
        Assert.Equal(EstadoAsignacion.POR_ASIGNAR, paqueteActualizado.Estado); // I9 (RN-17)
    }

    [Fact]
    public async Task Handle_sin_paquetes_no_resueltos_no_marca_nada()
    {
        // Arrange
        var paquete = PaqueteRecibidoBuilder.PorAsignar() with { Estado = EstadoAsignacion.ASIGNADO };
        var paquetesRecibidos = new PaqueteRecibidoStoreFake();
        paquetesRecibidos.Agregar(paquete);

        var policy = new ReponerPaquetesAlCancelarRutaPolicy(paquetesRecibidos);
        var notification = new RutaCancelada(
            RutaId.New(),
            RepartidorId.New(),
            "Vehiculo averiado",
            Array.Empty<PaqueteId>());

        // Act
        await policy.Handle(notification, CancellationToken.None);

        // Assert
        var paqueteSinCambios = Assert.Single(paquetesRecibidos.Paquetes);
        Assert.Equal(EstadoAsignacion.ASIGNADO, paqueteSinCambios.Estado); // sin efectos (UT-06)
    }
}
