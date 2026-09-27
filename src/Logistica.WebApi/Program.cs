using System.Globalization;
using System.Text.Json.Serialization;
using Logistica.Application.PaquetesRecibidos;
using Logistica.Infrastructure;
using Logistica.Infrastructure.Persistence;
using Logistica.WebApi.Middleware;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

// 1. Cultura por defecto del hilo, EN LA PRIMERA LINEA, antes de construir el builder (§11.4,
// §18.5 DESIGN.md): el binding [FromForm] de decimal? usa CurrentCulture, y en un host es-BO
// "-17.7654" se leeria como un numero fuera de rango.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// 2. Enums como string en MAYUSCULAS sobre el cable, nunca ordinal (§12.11 del maestro). Es uno
// de los DOS lugares donde se registra JsonStringEnumConverter; el otro es
// LoggingIntegrationEventPublisher.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 3. Swagger, MediatR, DbContext, repositorios, store y UnitOfWork.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogisticaInfrastructure(builder.Configuration);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(Program).Assembly,
    typeof(ProcesarPaquetesListosCommand).Assembly,
    typeof(LogisticaDbContext).Assembly));

var app = builder.Build();

// Swagger y la aplicacion automatica de migraciones van juntos, solo en Development (§12.12 del
// maestro). El migrador se resuelve desde un scope ANTES de configurar el pipeline HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LogisticaDbContext>();
    db.Database.Migrate();
}

// 4. La carpeta se resuelve con ContentRootPath, igual que en LocalEvidenciaStorage (§11.7
// DESIGN.md): mezclar una ruta relativa con una absoluta hace que las evidencias se guarden bien
// y devuelvan 404 al consultarlas, justo el sintoma que RN-16 no puede permitirse.
var carpetaEvidencias = Path.Combine(app.Environment.ContentRootPath, "evidencias");
Directory.CreateDirectory(carpetaEvidencias);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(carpetaEvidencias),
    RequestPath = "/evidencias"
});

app.UseHttpsRedirection();

app.UseAuthorization();

// 6. Antes de MapControllers: unico lugar donde una DomainException (o ArgumentException, red de
// seguridad de INC-S2) se traduce a {codigo, mensaje} (§11.9 DESIGN.md, §12.1 del maestro).
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
