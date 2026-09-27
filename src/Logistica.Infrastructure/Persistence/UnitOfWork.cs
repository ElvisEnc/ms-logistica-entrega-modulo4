using Joseco.DDD.Core.Abstractions;
using MediatR;

namespace Logistica.Infrastructure.Persistence;

public sealed class UnitOfWork(LogisticaDbContext context, IPublisher publisher) : IUnitOfWork
{
    // §12.6 del maestro, §14.4 DESIGN.md: recoge los DomainEvents del ChangeTracker, los limpia,
    // los publica por MediatR y SOLO ENTONCES llama a SaveChangesAsync. La limpieza antes de
    // publicar evita que un evento se emita dos veces si una politica vuelve a tocar la misma
    // entidad. Las politicas de liberacion mutan un Repartidor ya trackeado por este mismo
    // DbContext, de modo que su cambio entra en el mismo SaveChanges.
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var entradas = context.ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .ToList();

        var eventos = entradas.SelectMany(e => e.Entity.DomainEvents).ToList();

        foreach (var entrada in entradas)
            entrada.Entity.ClearDomainEvents();

        foreach (var evento in eventos)
            await publisher.Publish(evento, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
