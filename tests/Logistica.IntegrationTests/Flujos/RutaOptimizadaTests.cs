using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Logistica.Application.IntegrationEvents;
using Logistica.IntegrationTests.Setup;

namespace Logistica.IntegrationTests.Flujos;

// IT-03: un trait por nivel y uno por flujo.
[Trait("Capa", "Integracion")]
[Trait("Flujo", "F1-RutaOptimizada")]
public sealed class RutaOptimizadaTests
    : IClassFixture<LogisticaWebApplicationFactory>, IAsyncLifetime
{
    // IT-08: fecha fija en el pasado, distinta de F2 (2025-06-15) y F3 (2025-07-10).
    private static readonly DateOnly FechaEntrega = new(2025, 8, 20);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly LogisticaWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RutaOptimizadaTests(LogisticaWebApplicationFactory factory)
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
    // Fila 1 — Camino correcto
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Optimizar_ruta_valida_devuelve_200_y_publica_RutaOptimizadaGeneradaIntegrationEvent_con_paradas_ordenadas()
    {
        // Arrange — precondicion: ruta PENDIENTE lista para optimizar
        var repartidorId = await PrecondicionHelper.CrearRepartidorAsync(_client);
        var paqueteId    = await PrecondicionHelper.CrearPaqueteAsync(_client, FechaEntrega);

        var crearBody = new
        {
            Fecha        = FechaEntrega.ToString("yyyy-MM-dd"),
            RepartidorId = repartidorId,
            PaqueteIds   = new[] { paqueteId }
        };
        var crearResponse = await _client.PostAsJsonAsync("/api/rutas", crearBody);
        crearResponse.EnsureSuccessStatusCode();
        var rutaId = await crearResponse.Content.ReadFromJsonAsync<Guid>();

        // Limpiar eventos de la precondicion para afirmar solo el evento del optimizar (IT-07).
        _factory.Publisher.Clear();

        // Act
        var optimizarBody = new
        {
            Calle      = "Deposito Central",
            Zona       = "Centro",
            Ciudad     = "Santa Cruz",
            Referencia = (string?)null,
            Latitud    = -17.78m,
            Longitud   = -63.18m
        };
        var optimizarResponse = await _client.PostAsJsonAsync($"/api/rutas/{rutaId}/optimizar", optimizarBody);

        // Assert — HTTP 200 (IT-05)
        Assert.Equal(HttpStatusCode.OK, optimizarResponse.StatusCode);

        // Assert — RutaOptimizadaGeneradaIntegrationEvent capturado (IT-07): tipo y payload completo
        var events = _factory.Publisher.GetEvents<RutaOptimizadaGeneradaIntegrationEvent>();
        Assert.Single(events);

        var evt = events[0];

        // Los 4 campos del record (MAPA §2, fila 1)
        Assert.Equal(rutaId,       evt.RutaId);
        Assert.Equal(repartidorId, evt.RepartidorId);
        Assert.Equal(FechaEntrega, evt.Fecha);
        Assert.NotEmpty(evt.Paradas);

        // Paradas: la unica parada lleva el paqueteId correcto y sus campos no estan vacios
        var parada = evt.Paradas.Single();
        Assert.Equal(paqueteId, parada.PaqueteId);
        Assert.NotEqual(Guid.Empty, parada.ParadaId);
        Assert.NotEqual(Guid.Empty, parada.PacienteId);
        Assert.False(string.IsNullOrEmpty(parada.PacienteNombre));
        Assert.NotNull(parada.DireccionEntrega);

        // Paradas ordenadas por Orden (MAPA §2, fila 1; IT-07): la secuencia recibida
        // debe ser identica a la secuencia ordenada por Orden ascendente.
        var ordenRecibido = evt.Paradas.Select(p => p.Orden).ToList();
        var ordenEsperado = evt.Paradas.OrderBy(p => p.Orden).Select(p => p.Orden).ToList();
        Assert.Equal(ordenEsperado, ordenRecibido);
    }

    // ---------------------------------------------------------------------------
    // Fila 2 — Camino incorrecto: repartidor con ruta activa
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CrearRuta_con_repartidor_con_ruta_activa_devuelve_409_I11_y_ruta_original_sin_cambios()
    {
        // I11 (RN-27) — CrearRutaCommandHandler comprueba TieneRutaActivaAsync

        // Arrange — crear repartidor y primera ruta, optimizarla para que sea activa (OPTIMIZADA)
        var repartidorId = await PrecondicionHelper.CrearRepartidorAsync(_client);
        var paquete1Id   = await PrecondicionHelper.CrearPaqueteAsync(_client, FechaEntrega);

        var crearBody1 = new
        {
            Fecha        = FechaEntrega.ToString("yyyy-MM-dd"),
            RepartidorId = repartidorId,
            PaqueteIds   = new[] { paquete1Id }
        };
        var crearResponse1 = await _client.PostAsJsonAsync("/api/rutas", crearBody1);
        crearResponse1.EnsureSuccessStatusCode();
        var rutaId1 = await crearResponse1.Content.ReadFromJsonAsync<Guid>();

        var optimizarBody = new
        {
            Calle      = "Deposito Central",
            Zona       = "Centro",
            Ciudad     = "Santa Cruz",
            Referencia = (string?)null,
            Latitud    = -17.78m,
            Longitud   = -63.18m
        };
        var optimizarResponse = await _client.PostAsJsonAsync($"/api/rutas/{rutaId1}/optimizar", optimizarBody);
        optimizarResponse.EnsureSuccessStatusCode();

        // Capturar estado de la ruta original antes del segundo intento (IT-05)
        var rutaOriginal = await GetRutaAsync(rutaId1);

        // Act — segundo intento con el mismo repartidor en la misma fecha
        var paquete2Id = await PrecondicionHelper.CrearPaqueteAsync(_client, FechaEntrega);
        var crearBody2 = new
        {
            Fecha        = FechaEntrega.ToString("yyyy-MM-dd"),
            RepartidorId = repartidorId,
            PaqueteIds   = new[] { paquete2Id }
        };
        var crearResponse2 = await _client.PostAsJsonAsync("/api/rutas", crearBody2);

        // Assert — HTTP 409 (IT-05, IT-06: ErrorType.Conflict -> 409)
        Assert.Equal(HttpStatusCode.Conflict, crearResponse2.StatusCode);

        // Assert — codigo exacto del catalogo (IT-06): RutaErrors.RepartidorConRutaActiva()
        var errorJson = await crearResponse2.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(errorJson, JsonOpts)!;
        Assert.Equal("I11_REPARTIDOR_CON_RUTA_ACTIVA", error.Codigo); // IT-06

        // Assert — ruta original sin cambios (IT-05): releer desde DB via GET /api/rutas/{id}
        var rutaFinal = await GetRutaAsync(rutaId1);
        Assert.Equal(rutaOriginal.Estado,   rutaFinal.Estado);
        Assert.Equal(rutaOriginal.Paradas.Count, rutaFinal.Paradas.Count);
        Assert.Equal(rutaOriginal.Paradas[0].PaqueteId, rutaFinal.Paradas[0].PaqueteId);
    }

    // ---------------------------------------------------------------------------
    // Fila 3 — Camino incorrecto: repartidor inexistente
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task CrearRuta_con_repartidor_inexistente_devuelve_404_REPARTIDOR_NO_ENCONTRADO_y_paquete_sigue_POR_ASIGNAR()
    {
        // Arrange — paquete existente, repartidorId que no existe en la base
        var paqueteId             = await PrecondicionHelper.CrearPaqueteAsync(_client, FechaEntrega);
        var repartidorInexistente = Guid.NewGuid(); // Guid que no corresponde a ningun repartidor

        // Act
        var crearBody = new
        {
            Fecha        = FechaEntrega.ToString("yyyy-MM-dd"),
            RepartidorId = repartidorInexistente,
            PaqueteIds   = new[] { paqueteId }
        };
        var crearResponse = await _client.PostAsJsonAsync("/api/rutas", crearBody);

        // Assert — HTTP 404 (IT-05, IT-06: ErrorType.NotFound -> 404)
        Assert.Equal(HttpStatusCode.NotFound, crearResponse.StatusCode);

        // Assert — codigo exacto del catalogo (IT-06): RepartidorErrors.NoEncontrado()
        var errorJson = await crearResponse.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ErrorResponse>(errorJson, JsonOpts)!;
        Assert.Equal("REPARTIDOR_NO_ENCONTRADO", error.Codigo); // IT-06

        // Assert — paquete sigue POR_ASIGNAR (IT-05): releer desde DB via GET /api/paquetes
        var fechaStr     = FechaEntrega.ToString("yyyy-MM-dd");
        var paquetesJson = await _client.GetStringAsync(
            $"/api/paquetes?fecha={fechaStr}&estado=POR_ASIGNAR");
        var paquetes = JsonSerializer.Deserialize<PaqueteResumen[]>(paquetesJson, JsonOpts)!;
        Assert.Contains(paquetes, p => p.PaqueteId == paqueteId);
    }

    // ---------------------------------------------------------------------------
    // Helpers privados
    // ---------------------------------------------------------------------------

    private async Task<RutaDetalleResponse> GetRutaAsync(Guid rutaId)
    {
        var response = await _client.GetAsync($"/api/rutas/{rutaId}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RutaDetalleResponse>(json, JsonOpts)!;
    }

    // ---------------------------------------------------------------------------
    // DTOs locales de deserializacion — reflejan el contrato JSON de los endpoints
    // ---------------------------------------------------------------------------

    private sealed record RutaDetalleResponse(
        Guid RutaId,
        Guid RepartidorId,
        DateOnly Fecha,
        string Estado,
        bool Optimizada,
        decimal DistanciaTotalKm,
        int TiempoEstimadoMin,
        List<ParadaResumenResponse> Paradas);

    private sealed record ParadaResumenResponse(
        Guid ParadaId,
        int Orden,
        Guid PaqueteId);

    private sealed record PaqueteResumen(
        Guid PaqueteId,
        string Estado);

    private sealed record ErrorResponse(string Codigo, string Mensaje);
}
