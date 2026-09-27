using Joseco.DDD.Core.Abstractions;

namespace Logistica.Application.Tests.TestSupport;

// Fake de IUnitOfWork (UT-12, MAPA §6): expone si CommitAsync se llamo, para las aserciones
// "sin efectos" de UT-06 en los tests de rama de error de los command handlers.
public sealed class UnitOfWorkFake : IUnitOfWork
{
    public bool Committed { get; private set; }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Committed = true;
        return Task.CompletedTask;
    }
}
