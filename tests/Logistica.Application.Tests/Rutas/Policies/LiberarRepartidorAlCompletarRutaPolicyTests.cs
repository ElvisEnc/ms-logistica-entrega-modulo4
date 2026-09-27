using Joseco.DDD.Core.Results;
using Logistica.Application.Rutas.Policies;
using Logistica.Application.Tests.TestSupport;
using Logistica.Domain.Rutas;
using Logistica.Domain.Rutas.Events;
using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.Rutas.Policies;

// §8.2 DESIGN.md: la politica reacciona al evento de DOMINIO RutaCompletada y libera al
// repartidor (I11). No llama a IUnitOfWork.CommitAsync (comentario del codigo real): su
// unico efecto observable en este nivel es el estado de Repartidor.Disponible.
[Trait("Capa", "Unit")]
public class LiberarRepartidorAlCompletarRutaPolicyTests
{
    [Fact]
    public async Task Handle_libera_al_repartidor_de_la_ruta_completada()
    {
        // Arrange
        var repartidor = RepartidorBuilder.Disponible();
        repartidor.AsignarRuta(); // lo deja no disponible, como quedaria al crear la ruta
        var repartidores = new RepartidorRepositoryFake();
        repartidores.Agregar(repartidor);

        var policy = new LiberarRepartidorAlCompletarRutaPolicy(repartidores);
        var notification = new RutaCompletada(RutaId.New(), repartidor.RepartidorId, EstadoRuta.COMPLETADA);

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
        var policy = new LiberarRepartidorAlCompletarRutaPolicy(repartidores);
        var repartidorId = RepartidorId.New();
        var notification = new RutaCompletada(RutaId.New(), repartidorId, EstadoRuta.COMPLETADA);

        // Act
        var excepcion = await Assert.ThrowsAsync<DomainException>(() =>
            policy.Handle(notification, CancellationToken.None));

        // Assert
        Assert.Equal("REPARTIDOR_NO_ENCONTRADO", excepcion.Error.Code);
        Assert.Equal(ErrorType.NotFound, excepcion.Error.Type);
        Assert.Null(await repartidores.GetByIdAsync(repartidorId.Value)); // sin efectos (UT-06)
    }
}
