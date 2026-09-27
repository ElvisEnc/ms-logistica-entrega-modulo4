using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Logistica.Application.IntegrationEvents;
using Logistica.Domain.Shared;
using Logistica.IntegrationTests.Setup;

namespace Logistica.IntegrationTests.Flujos;

// IT-03: un trait por nivel y uno por flujo.
[Trait("Capa", "Integracion")]
[Trait("Flujo", "F2-ConfirmacionDeEntrega")]
public sealed class ConfirmacionDeEntregaTests
    : IClassFixture<LogisticaWebApplicationFactory>, IAsyncLifetime
{
    // IT-08: fecha fija en el pasado, no DateTime.Now.
    private static readonly DateOnly FechaEntrega = new(2025, 6, 15);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly LogisticaWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ConfirmacionDeEntregaTests(LogisticaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Se ejecuta antes de CADA test: limpia los eventos capturados (IT-04, IT-02).
    public Task InitializeAsync()
    {
        _factory.Publisher.Clear();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ---------------------------------------------------------------------------
    // Fila 4 — Camino correcto
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Confirmar_parada_en_camino_devuelve_200_y_parada_queda_ENTREGADO_con_constancia_y_evento_publicado()
    {
        // Arrange — precondicion: ruta EN_CAMINO (I3)
        var repartidorId = await PrecondicionHelper.CrearRepartidorAsync(_client);
        var paqueteId = await PrecondicionHelper.CrearPaqueteAsync(_client, FechaEntrega);
        var (rutaId, paradaId) = await PrecondicionHelper.CrearYIniciarRutaAsync(
            _client, repartidorId, paqueteId, FechaEntrega);

        // Limpiar eventos de la precondicion (RutaOptimizadaGeneradaIntegrationEvent)
        // para afirmar solo el evento de la confirmacion (IT-07).
        _factory.Publisher.Clear();

        // Act
        var confirmarResponse = await ConfirmarEntregaAsync(rutaId, paradaId, "Receptor Prueba");

        // Assert — HTTP 200 (IT-05)
        Assert.Equal(HttpStatusCode.OK, confirmarResponse.StatusCode);

        // Assert — estado persistido (IT-05): releer desde DB via GET /api/rutas/{id}
        var ruta = await GetRutaAsync(rutaId);
        var parada = ruta.Paradas.Single(p => p.ParadaId == paradaId);

        Assert.Equal("ENTREGADO", parada.Estado);           // parada queda ENTREGADO
        Assert.NotNull(parada.Constancia);                  // Constancia asignada (I5)
        Assert.Null(parada.Incidencia);                     // Incidencia = null (I5, RN-16)
        Assert.Equal("Receptor Prueba", parada.Constancia!.ReceptorNombre);
        Assert.False(string.IsNullOrEmpty(parada.Constancia.UrlEvidencia));

        // Assert — EntregaConfirmadaIntegrationEvent capturado (IT-07)
        var events = _factory.Publisher.GetEvents<EntregaConfirmadaIntegrationEvent>();
        Assert.Single(events);  // exactamente uno, no solo "algo"

        var evt = events[0];
        Assert.Equal(rutaId, evt.RutaId);
        Assert.Equal(paradaId, evt.ParadaId);
        Assert.Equal(paqueteId, evt.PaqueteId);
        Assert.NotEqual(Guid.Empty, evt.PacienteId);
        Assert.Equal("Paciente Prueba", evt.PacienteNombre);

        // Constancia ANIDADA (SISTEMA.md §9.13): nunca aplanada
        Assert.NotNull(evt.Constancia);
        Assert.Equal(TipoConstancia.FOTO, evt.Constancia.Tipo);
        Assert.Equal("Receptor Prueba", evt.Constancia.ReceptorNombre);
        Assert.False(string.IsNullOrEmpty(evt.Constancia.UrlEvidencia));
        Assert.True(evt.Constancia.FechaHora > DateTime.MinValue);
        Assert.Null(evt.Constancia.CoordenadasConfirmacion); // presente pero null: no se enviaron lat/lon
    }

    // ---------------------------------------------------------------------------
    // Fila 5 — Camino incorrecto: confirmar dos veces la misma parada
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Confirmar_parada_ya_ENTREGADO_devuelve_409_I4_TRANSICION_INVALIDA_y_constancia_no_se_sobrescribe()
    {
        // I4 (RN-16) via ParadaEntregaErrors.TransicionInvalida()

        // Arrange — precondicion: ruta EN_CAMINO
        var repartidorId = await PrecondicionHelper.CrearRepartidorAsync(_client);
        var paqueteId = await PrecondicionHelper.CrearPaqueteAsync(_client, FechaEntrega);
        var (rutaId, paradaId) = await PrecondicionHelper.CrearYIniciarRutaAsync(
            _client, repartidorId, paqueteId, FechaEntrega);

        // Primer confirmar — debe ser 200
        var primerConfirmar = await ConfirmarEntregaAsync(rutaId, paradaId, "Receptor Original");
        Assert.Equal(HttpStatusCode.OK, primerConfirmar.StatusCode);

        // Capturar constancia original desde DB antes del segundo intento (IT-05)
        var rutaTrasElPrimero = await GetRutaAsync(rutaId);
        var paradaTrasElPrimero = rutaTrasElPrimero.Paradas.Single(p => p.ParadaId == paradaId);
        var urlEvidenciaOriginal = paradaTrasElPrimero.Constancia!.UrlEvidencia;
        var receptorNombreOriginal = paradaTrasElPrimero.Constancia!.ReceptorNombre;

        // Act — segundo confirmar sobre la misma parada ya ENTREGADO
        var segundoConfirmar = await ConfirmarEntregaAsync(rutaId, paradaId, "Receptor Diferente");

        // Assert — HTTP 409 (IT-05, IT-06: ErrorType.Conflict -> 409)
        Assert.Equal(HttpStatusCode.Conflict, segundoConfirmar.StatusCode);

        // Assert — codigo exacto del catalogo (IT-06): ParadaEntregaErrors.TransicionInvalida()
        var errorJson = await segundoConfirmar.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(errorJson, JsonOpts)!;
        Assert.Equal("I4_TRANSICION_INVALIDA", error.Codigo); // IT-06

        // Assert — estado persistido SIN cambios (IT-05): releer desde DB
        var rutaFinal = await GetRutaAsync(rutaId);
        var paradaFinal = rutaFinal.Paradas.Single(p => p.ParadaId == paradaId);

        Assert.Equal("ENTREGADO", paradaFinal.Estado);
        Assert.Equal(urlEvidenciaOriginal, paradaFinal.Constancia!.UrlEvidencia);     // no sobrescrita
        Assert.Equal(receptorNombreOriginal, paradaFinal.Constancia!.ReceptorNombre); // no sobrescrita
    }

    // ---------------------------------------------------------------------------
    // Helpers privados
    // ---------------------------------------------------------------------------

    private async Task<HttpResponseMessage> ConfirmarEntregaAsync(
        Guid rutaId, Guid paradaId, string receptorNombre)
    {
        // Multipart form-data (§11.4 DESIGN.md): Tipo no pasa por JsonStringEnumConverter.
        // El valor del enum en SCREAMING_SNAKE_CASE se envía como string (§5.5, CLAUDE.md).
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("FOTO"), "Tipo");
        form.Add(new StringContent(receptorNombre), "ReceptorNombre");

        // Archivo PNG mínimo — extensión .png es válida (EvidenciaArchivoValidator)
        var archivoBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG signature
        var archivoContent = new ByteArrayContent(archivoBytes);
        archivoContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(archivoContent, "Archivo", "evidencia.png");

        return await _client.PostAsync($"/api/rutas/{rutaId}/paradas/{paradaId}/confirmar", form);
    }

    private async Task<RutaDetalleResponse> GetRutaAsync(Guid rutaId)
    {
        var response = await _client.GetAsync($"/api/rutas/{rutaId}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RutaDetalleResponse>(json, JsonOpts)!;
    }

    // ---------------------------------------------------------------------------
    // DTOs locales de deserializacion — reflejan el contrato JSON del GET /api/rutas/{id}
    // ---------------------------------------------------------------------------

    private sealed record RutaDetalleResponse(
        Guid RutaId,
        Guid RepartidorId,
        DateOnly Fecha,
        string Estado,
        bool Optimizada,
        decimal DistanciaTotalKm,
        int TiempoEstimadoMin,
        List<ParadaDetalleResponse> Paradas);

    private sealed record ParadaDetalleResponse(
        Guid ParadaId,
        int Orden,
        Guid PaqueteId,
        Guid PacienteId,
        string PacienteNombre,
        JsonElement DireccionEntrega,
        string Estado,
        ConstanciaResponse? Constancia,
        JsonElement? Incidencia);

    private sealed record ConstanciaResponse(
        string Tipo,
        string UrlEvidencia,
        string ReceptorNombre,
        DateTime FechaHora,
        JsonElement? CoordenadasConfirmacion);

    private sealed record ErrorResponse(string Codigo, string Mensaje);
}
