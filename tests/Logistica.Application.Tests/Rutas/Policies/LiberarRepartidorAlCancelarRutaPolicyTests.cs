using Joseco.DDD.Core.Results;
using Logistica.Application.Rutas.Policies;
using Logistica.Application.Tests.TestSupport;
using Logistica.Domain.Rutas.Events;
using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.Rutas.Policies;

// §8.2 DESIGN.md: simetrica a LiberarRepartidorAlCompletarRutaPolicy, reaccionando a
// RutaCancelada en vez de RutaCompletada.
[Trait("Capa", "Unit")]
public class LiberarRepartidorAlCancelarRutaPolicyTests
{
    [Fact]
    public async Task Handle_libera_al_repartidor_de_la_ruta_cancelada()
    {
        // Arrange
        var repartidor = RepartidorBuilder.Disponible();
        repartidor.AsignarRuta(); // lo deja no disponible, como quedaria al crear la ruta
        var repartidores = new RepartidorRepositoryFake();
        repartidores.Agregar(repartidor);

        var policy = new LiberarRepartidorAlCancelarRutaPolicy(repartidores);
        var notification = new RutaCancelada(
            RutaId.New(),
            repartidor.RepartidorId,
            "Vehiculo averiado",
            Array.Empty<PaqueteId>());

        // Act
        await policy.Handle(notification, CancellationToken.None);

        // Assert
        Assert.True(repartidor.EstaDisponible()); // I11 (RN-27)
    }

    [Fact]
    public async Task Handle_sin_repartidor_lanza_REPARTIDOR_NO_ENCONTRADO()
    {
        // Arrange
        var repartidores = new RepartidorRepositoryFake();
        var policy = new LiberarRepartidorAlCancelarRutaPolicy(repartidores);
        var repartidorId = RepartidorId.New();
        var notification = new RutaCancelada(
            RutaId.New(),
            repartidorId,
            "Vehiculo averiado",
            Array.Empty<PaqueteId>());

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            policy.Handle(notification, CancellationToken.None));

        // Assert
        Assert.Equal("REPARTIDOR_NO_ENCONTRADO", excepcion.Error.Code);
        Assert.Equal(ErrorType.NotFound, excepcion.Error.Type);
        Assert.Null(await repartidores.GetByIdAsync(repartidorId.Value)); // sin efectos (UT-06)
    }
}
