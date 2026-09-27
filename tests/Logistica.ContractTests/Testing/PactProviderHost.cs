using Logistica.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Logistica.ContractTests.Testing;

// CT-02/CT-03: shim HTTP de SOLO TEST para verificar como proveedor bajo Pact HTTP (INC-2,
// opcion B). Kestrel real en un socket TCP real -- PactNet exige eso para la verificacion
// HTTP, no admite un TestServer en memoria de Microsoft.AspNetCore.Mvc.Testing. No define
// ningun endpoint de produccion: los dos POST de aqui existen solo para que el verificador de
// Pact tenga algo real que invocar. El ensamblado de MediatR se descubre con un tipo PUBLICO
// de Logistica.Application (IIntegrationEventPublisher); los handlers reales que se registran
// son `internal` y MediatR los encuentra por reflexion sin necesitar visibilidad ampliada.
public sealed class PactProviderHost : IAsyncDisposable
{
    private WebApplication? _app;

    public Uri ServerUri { get; private set; } = null!;

    public async Task StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        // El escaneo de ensamblado de MediatR registra TODOS los handlers de
        // Logistica.Application, no solo los dos que este shim invoca. La validacion eager de
        // Build() (activa por defecto en Development) intentaria resolver las dependencias de
        // todos ellos (repositorios, UnitOfWork, etc.) que este shim no registra a proposito
        // (CT-03: solo lo minimo para ejercitar el traductor real). Se desactiva esa validacion
        // eager; MediatR sigue resolviendo en runtime solo el handler del evento publicado.
        builder.Host.UseDefaultServiceProvider((_, options) =>
        {
            options.ValidateOnBuild = false;
            options.ValidateScopes = false;
        });

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IIntegrationEventPublisher).Assembly));

        var capturador = new CapturingIntegrationEventPublisher();
        builder.Services.AddSingleton(capturador);
        builder.Services.AddSingleton<IIntegrationEventPublisher>(capturador);

        _app = builder.Build();

        _app.MapPost("/pact-provider/entrega-confirmada", async (IPublisher publisher, CapturingIntegrationEventPublisher captura) =>
        {
            captura.Limpiar();
            await publisher.Publish(EscenariosProveedor.EntregaConfirmada());
            return Results.Text(captura.UltimoPayloadJson!, "application/json; charset=utf-8");
        });

        _app.MapPost("/pact-provider/incidencia", async (IPublisher publisher, CapturingIntegrationEventPublisher captura) =>
        {
            captura.Limpiar();
            await publisher.Publish(EscenariosProveedor.IncidenciaEntregaRegistrada());
            return Results.Text(captura.UltimoPayloadJson!, "application/json; charset=utf-8");
        });

        await _app.StartAsync();

        var direccion = _app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.First();
        ServerUri = new Uri(direccion);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
            await _app.DisposeAsync();
    }
}
