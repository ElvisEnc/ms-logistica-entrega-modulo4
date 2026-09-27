using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Logistica.IntegrationTests.Setup;

/// <summary>
/// Helpers HTTP para construir la precondicion comun de los flujos F2 y F3.
/// Todos los metodos lanzan excepcion si alguna llamada HTTP falla: si la precondicion
/// no se puede construir, el test debe fallar con un mensaje claro, no con un NullRef.
/// </summary>
public static class PrecondicionHelper
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>POST /api/repartidores — devuelve el repartidorId creado.</summary>
    public static async Task<Guid> CrearRepartidorAsync(HttpClient client)
    {
        var body = new
        {
            Nombre = "Repartidor Integracion",
            Telefono = "70100001",
            TipoVehiculo = "MOTO",
            Placa = "MTG-001",
            CapacidadPaquetes = 10
        };

        var response = await client.PostAsJsonAsync("/api/repartidores", body);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    /// <summary>
    /// POST /api/integracion/paquetes-listos — registra un paquete POR_ASIGNAR y devuelve
    /// el paqueteId generado por el caller (el mismo que se envio en el cuerpo).
    /// </summary>
    public static async Task<Guid> CrearPaqueteAsync(HttpClient client, DateOnly fecha)
    {
        var paqueteId = Guid.NewGuid();
        var pacienteId = Guid.NewGuid();
        var contratoCateringId = Guid.NewGuid();
        var fechaStr = fecha.ToString("yyyy-MM-dd");

        var direccion = new
        {
            Calle = "Av. Cristo Redentor",
            Zona = "Norte",
            Ciudad = "Santa Cruz",
            Referencia = (string?)null,
            Coordenadas = new { Latitud = -17.77m, Longitud = -63.18m }
        };

        var body = new
        {
            Fecha = fechaStr,
            Paquetes = new[]
            {
                new
                {
                    PaqueteId = paqueteId,
                    PacienteId = pacienteId,
                    PacienteNombre = "Paciente Prueba",
                    DireccionEntrega = direccion,
                    ContratoCateringId = contratoCateringId,
                    Etiqueta = new
                    {
                        PaqueteId = paqueteId,
                        NombrePaciente = "Paciente Prueba",
                        NroIdentificacion = "9900112233",
                        DireccionEntrega = direccion,
                        Fecha = fechaStr,
                        CodigoQR = (string?)null
                    }
                }
            }
        };

        var response = await client.PostAsJsonAsync("/api/integracion/paquetes-listos", body);
        response.EnsureSuccessStatusCode();

        return paqueteId;
    }

    /// <summary>
    /// POST /api/rutas → POST /api/rutas/{id}/optimizar → POST /api/rutas/{id}/iniciar
    /// Devuelve (rutaId, paradaId) con la ruta en estado EN_CAMINO.
    /// </summary>
    public static async Task<(Guid rutaId, Guid paradaId)> CrearYIniciarRutaAsync(
        HttpClient client, Guid repartidorId, Guid paqueteId, DateOnly fecha)
    {
        // Crear ruta
        var crearBody = new
        {
            Fecha = fecha.ToString("yyyy-MM-dd"),
            RepartidorId = repartidorId,
            PaqueteIds = new[] { paqueteId }
        };
        var crearResponse = await client.PostAsJsonAsync("/api/rutas", crearBody);
        crearResponse.EnsureSuccessStatusCode();
        var rutaId = await crearResponse.Content.ReadFromJsonAsync<Guid>();

        // Optimizar (origen: deposito central)
        var optimizarBody = new
        {
            Calle = "Deposito Central",
            Zona = "Centro",
            Ciudad = "Santa Cruz",
            Referencia = (string?)null,
            Latitud = -17.78m,
            Longitud = -63.18m
        };
        var optimizarResponse = await client.PostAsJsonAsync($"/api/rutas/{rutaId}/optimizar", optimizarBody);
        optimizarResponse.EnsureSuccessStatusCode();

        // Iniciar
        var iniciarResponse = await client.PostAsync($"/api/rutas/{rutaId}/iniciar", content: null);
        iniciarResponse.EnsureSuccessStatusCode();

        // Obtener paradaId del GET
        var rutaDetailResponse = await client.GetAsync($"/api/rutas/{rutaId}");
        rutaDetailResponse.EnsureSuccessStatusCode();
        var json = await rutaDetailResponse.Content.ReadAsStringAsync();
        var rutaDetail = JsonSerializer.Deserialize<RutaDetalleResponse>(json, JsonOpts)!;
        var paradaId = rutaDetail.Paradas.First().ParadaId;

        return (rutaId, paradaId);
    }

    // DTOs internos para la deserializacion del GET /api/rutas/{id}
    private sealed record RutaDetalleResponse(
        Guid RutaId,
        Guid RepartidorId,
        DateOnly Fecha,
        string Estado,
        bool Optimizada,
        decimal DistanciaTotalKm,
        int TiempoEstimadoMin,
        List<ParadaResponse> Paradas);

    private sealed record ParadaResponse(Guid ParadaId, int Orden, Guid PaqueteId);
}
