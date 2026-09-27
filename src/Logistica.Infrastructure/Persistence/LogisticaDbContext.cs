using Logistica.Application.PaquetesRecibidos;
using Logistica.Domain.Repartidores;
using Logistica.Domain.Rutas;
using Logistica.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Logistica.Infrastructure.Persistence;

public sealed class LogisticaDbContext(DbContextOptions<LogisticaDbContext> options) : DbContext(options)
{
    public DbSet<RutaEntrega> Rutas => Set<RutaEntrega>();
    public DbSet<Repartidor> Repartidores => Set<Repartidor>();
    public DbSet<PaqueteRecibido> PaquetesRecibidos => Set<PaqueteRecibido>();

    // Sin DbSet<ParadaEntrega> publico: se accede unicamente por navegacion desde RutaEntrega,
    // coherente con que no existe (ni debe existir) un repositorio de paradas (§14.4 DESIGN.md).

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RutaEntregaConfig());
        modelBuilder.ApplyConfiguration(new ParadaEntregaConfig());
        modelBuilder.ApplyConfiguration(new RepartidorConfig());
        modelBuilder.ApplyConfiguration(new PaqueteRecibidoConfig());
    }
}
