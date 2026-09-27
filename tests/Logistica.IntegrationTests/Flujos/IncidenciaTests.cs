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
[Trait("Flujo", "F3-Incidencia")]
public sealed class IncidenciaTests
    : IClassFixture<LogisticaWebApplicationFactory>, IAsyncLifetime
{
    // IT-08: fecha fija en el pasado, no DateTime.Now.
    private static readonly DateOnly FechaEntrega = new(2025, 7, 10);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly LogisticaWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public IncidenciaTests(LogisticaWebApplicationFactory factory)
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
    // Fila 6 — Camino correcto
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Reportar_incidencia_en_parada_EN_CAMINO_devuelve_200_y_parada_queda_NO_ENTREGADO_con_incidencia_asignada_y_evento_publicado()
    {
        // Arrange — precondicion: ruta EN_CAMINO
        var repartidorId = await PrecondicionHelper.CrearRepartidorAsync(_client);
        var paqueteId = await PrecondicionHelper.CrearPaqueteAsync(_client, FechaEntrega);
        var (rutaId, paradaId) = await PrecondicionHelper.CrearYIniciarRutaAsync(
            _client, repartidorId, paqueteId, FechaEntrega);

        // Limpiar eventos de la precondicion (RutaOptimizadaGeneradaIntegrationEvent)
        // para afirmar solo el evento de la incidencia (IT-07).
        _factory.Publisher.Clear();

        // Act
        var incidenciaResponse = await ReportarIncidenciaAsync(
            rutaId, paradaId, MotivoIncidencia.PACIENTE_AUSENTE, "Paciente no estaba en casa");

        // Assert — HTTP 200 (IT-05)
        Assert.Equal(HttpStatusCode.OK, incidenciaResponse.StatusCode);

        // Assert — estado persistido (IT-05): releer desde DB via GET /api/rutas/{id}
        var ruta = await GetRutaAsync(rutaId);
        var parada = ruta.Paradas.Single(p => p.ParadaId == paradaId);

        Assert.Equal("NO_ENTREGADO", parada.Estado);    // parada queda NO_ENTREGADO
        Assert.NotNull(parada.Incidencia);               // Incidencia asignada
        Assert.Null(parada.Constancia);                  // Constancia = null (I5, RN-16)

        Assert.Equal(MotivoIncidencia.PACIENTE_AUSENTE, parada.Incidencia.Motivo);
        Assert.Equal("Paciente no estaba en casa", parada.Incidencia.Descripcion);
        Assert.True(parada.Incidencia.FechaHora > DateTime.MinValue);
        Assert.Null(parada.Incidencia.UrlFoto);          // no se adjunto archivo

        // Assert — IncidenciaEntregaRegistradaIntegrationEvent capturado (IT-07)
        // SISTEMA.md §9.14: DIEZ campos, no nueve; urlFoto presente aunque sea null.
        var events = _factory.Publisher.GetEvents<IncidenciaEntregaRegistradaIntegrationEvent>();
        Assert.Single(events); // exactamente uno, no solo "algo"

        var evt = events[0];
        Assert.Equal(rutaId, evt.RutaId);                               // campo 1
        Assert.Equal(paradaId, evt.ParadaId);                           // campo 2
        Assert.Equal(paqueteId, evt.PaqueteId);                         // campo 3
        Assert.NotEqual(Guid.Empty, evt.PacienteId);                    // campo 4: heredado del paquete (I14)
        Assert.Equal("Paciente Prueba", evt.PacienteNombre);            // campo 5
        Assert.NotEqual(Guid.Empty, evt.ContratoCateringId);            // campo 6: heredado del paquete (I14)
        Assert.Equal(MotivoIncidencia.PACIENTE_AUSENTE, evt.Motivo);    // campo 7: sin interpretar
        Assert.Equal("Paciente no estaba en casa", evt.Descripcion);    // campo 8
        Assert.True(evt.FechaHora > DateTime.MinValue);                 // campo 9
        Assert.Null(evt.UrlFoto);                                        // campo 10: null cuando no se adjunta archivo
    }

    // ---------------------------------------------------------------------------
    // Fila 7 — Camino incorrecto: incidencia sobre parada ya confirmada
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Reportar_incidencia_sobre_parada_ya_ENTREGADO_devuelve_409_I4_TRANSICION_INVALIDA_y_constancia_no_se_modifica()
    {
        // I4 (RN-16) via ParadaEntregaErrors.TransicionInvalida()

        // Arrange — precondicion: ruta EN_CAMINO
        var repartidorId = await PrecondicionHelper.CrearRepartidorAsync(_client);
        var paqueteId = await PrecondicionHelper.CrearPaqueteAsync(_client, FechaEntrega);
        var (rutaId, paradaId) = await PrecondicionHelper.CrearYIniciarRutaAsync(
            _client, repartidorId, paqueteId, FechaEntrega);

        // Confirmar la parada primero — debe ser 200
        var confirmarResponse = await ConfirmarEntregaAsync(rutaId, paradaId, "Receptor Original");
        Assert.Equal(HttpStatusCode.OK, confirmarResponse.StatusCode);

        // Capturar estado original desde DB antes del intento de incidencia (IT-05)
        var rutaTrasConfirmar = await GetRutaAsync(rutaId);
        var paradaTrasConfirmar = rutaTrasConfirmar.Paradas.Single(p => p.ParadaId == paradaId);
        Assert.Equal("ENTREGADO", paradaTrasConfirmar.Estado);
        var urlEvidenciaOriginal = paradaTrasConfirmar.Constancia!.UrlEvidencia;
        var receptorNombreOriginal = paradaTrasConfirmar.Constancia!.ReceptorNombre;

        // Act — intentar reportar incidencia sobre parada ya ENTREGADO
        var incidenciaResponse = await ReportarIncidenciaAsync(
            rutaId, paradaId, MotivoIncidencia.PACIENTE_AUSENTE,
            "Intento de incidencia sobre parada ya confirmada");

        // Assert — HTTP 409 (IT-05, IT-06: ErrorType.Conflict -> 409)
        Assert.Equal(HttpStatusCode.Conflict, incidenciaResponse.StatusCode);

        // Assert — codigo exacto del catalogo (IT-06): ParadaEntregaErrors.TransicionInvalida()
        var errorJson = await incidenciaResponse.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(errorJson, JsonOpts)!;
        Assert.Equal("I4_TRANSICION_INVALIDA", error.Codigo); // IT-06

        // Assert — estado persistido SIN cambios (IT-05): releer desde DB
        var rutaFinal = await GetRutaAsync(rutaId);
        var paradaFinal = rutaFinal.Paradas.Single(p => p.ParadaId == paradaId);

        Assert.Equal("ENTREGADO", paradaFinal.Estado);                               // sigue ENTREGADO
        Assert.Null(paradaFinal.Incidencia);                                          // no se asigno incidencia
        Assert.Equal(urlEvidenciaOriginal, paradaFinal.Constancia!.UrlEvidencia);    // constancia intacta
        Assert.Equal(receptorNombreOriginal, paradaFinal.Constancia!.ReceptorNombre); // receptor intacto
    }

    // ---------------------------------------------------------------------------
    // Helpers privados
    // ---------------------------------------------------------------------------

    /// <summary>
    /// POST /api/rutas/{rutaId}/paradas/{paradaId}/incidencia (multipart).
    /// El archivo es OPCIONAL en incidencia (§11.4 DESIGN.md): no se pasa cuando no se necesita.
    /// Motivo en SCREAMING_SNAKE_CASE — el binding multipart no pasa por JsonStringEnumConverter (§5.5, CLAUDE.md).
    /// </summary>
    private async Task<HttpResponseMessage> ReportarIncidenciaAsync(
        Guid rutaId, Guid paradaId, MotivoIncidencia motivo, string descripcion,
        byte[]? archivoBytes = null, string? nombreArchivo = null)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(motivo.ToString()), "Motivo");
        form.Add(new StringContent(descripcion), "Descripcion");

        if (archivoBytes is not null)
        {
            var archivoContent = new ByteArrayContent(archivoBytes);
            archivoContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(archivoContent, "Archivo", nombreArchivo ?? "evidencia.png");
        }

        return await _client.PostAsync($"/api/rutas/{rutaId}/paradas/{paradaId}/incidencia", form);
    }

    /// <summary>
    /// POST /api/rutas/{rutaId}/paradas/{paradaId}/confirmar (multipart).
    /// Necesario en la fila 7 para llevar la parada a ENTREGADO antes de intentar la incidencia.
    /// </summary>
    private async Task<HttpResponseMessage> ConfirmarEntregaAsync(
        Guid rutaId, Guid paradaId, string receptorNombre)
    {
        // Tipo en SCREAMING_SNAKE_CASE (§5.5, CLAUDE.md).
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("FOTO"), "Tipo");
        form.Add(new StringContent(receptorNombre), "ReceptorNombre");

        // Archivo PNG minimo — extension .png es valida (EvidenciaArchivoValidator).
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
        IncidenciaResponse? Incidencia);

    private sealed record ConstanciaResponse(
        string Tipo,
        string UrlEvidencia,
        string ReceptorNombre,
        DateTime FechaHora,
        JsonElement? CoordenadasConfirmacion);

    // Refleja IncidenciaEntregaDto (Application/Rutas/Queries/IncidenciaEntregaDto.cs).
    private sealed record IncidenciaResponse(
        MotivoIncidencia Motivo,
        string Descripcion,
        DateTime FechaHora,
        string? UrlFoto);

    private sealed record ErrorResponse(string Codigo, string Mensaje);
}
