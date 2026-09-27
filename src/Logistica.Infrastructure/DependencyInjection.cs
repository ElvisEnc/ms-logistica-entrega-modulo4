using Logistica.Application.Abstractions;
using Logistica.Domain.Repartidores;
using Logistica.Domain.Rutas;
using Logistica.Infrastructure.Messaging;
using Logistica.Infrastructure.Persistence;
using Logistica.Infrastructure.Persistence.Repositories;
using Logistica.Infrastructure.Storage;
using Joseco.DDD.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistica.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLogisticaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LogisticaDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddScoped<IRutaEntregaRepository, RutaEntregaRepository>();
        services.AddScoped<IRepartidorRepository, RepartidorRepository>();
        services.AddScoped<IPaqueteRecibidoStore, PaqueteRecibidoStore>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEvidenciaStorage, LocalEvidenciaStorage>();
        services.AddScoped<IIntegrationEventPublisher, LoggingIntegrationEventPublisher>();

        return services;
    }
}
