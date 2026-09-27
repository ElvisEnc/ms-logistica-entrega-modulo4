using System.Globalization;
using System.Text.Json;
using Logistica.Domain.Rutas;

namespace Logistica.Infrastructure.Queries;

// HU-39 (demo). §11.3 DESIGN.md: pagina autocontenida con Leaflet y teselas de OpenStreetMap
// desde CDN, sin claves de API. D-14 (§19.3): sin conexion a internet, la pagina se sirve con
// 200 pero no renderiza — aceptado para material de demostracion de RN-15.
// Si la ruta aun no fue optimizada, todas las paradas tienen Orden = 0 y se dibujan en orden de
// insercion: el mapa es una lectura, no valida I3.
internal static class MapaRutaHtmlBuilder
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Construir(RutaEntrega ruta)
    {
        var paradas = ruta.Paradas
            .OrderBy(p => p.Orden)
            .Select(p => new MarcadorParada(
                p.Orden,
                p.DireccionEntrega.Coordenadas.Latitud,
                p.DireccionEntrega.Coordenadas.Longitud,
                p.PacienteNombre,
                $"{p.DireccionEntrega.Calle}, {p.DireccionEntrega.Zona}, {p.DireccionEntrega.Ciudad}",
                p.Estado.ToString()))
            .ToList();

        var paradasJson = JsonSerializer.Serialize(paradas, SerializerOptions);

        var (centroLat, centroLng) = paradas.Count > 0
            ? (paradas[0].Lat, paradas[0].Lng)
            : (0m, 0m);

        var centroLatTexto = centroLat.ToString(CultureInfo.InvariantCulture);
        var centroLngTexto = centroLng.ToString(CultureInfo.InvariantCulture);

        return
            "<!DOCTYPE html>\n" +
            "<html lang=\"es\">\n" +
            "<head>\n" +
            "<meta charset=\"utf-8\" />\n" +
            $"<title>Ruta {ruta.RutaId.Value}</title>\n" +
            "<link rel=\"stylesheet\" href=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.css\" />\n" +
            "<style>\n" +
            "  body { font-family: sans-serif; margin: 0; }\n" +
            "  header { padding: 12px 16px; background: #1f2937; color: #fff; }\n" +
            "  #mapa { height: calc(100vh - 64px); }\n" +
            "</style>\n" +
            "</head>\n" +
            "<body>\n" +
            "<header>\n" +
            $"  <strong>Ruta {ruta.RutaId.Value}</strong> &middot; " +
            $"Repartidor {ruta.RepartidorId.Value} &middot; " +
            $"{ruta.Fecha:yyyy-MM-dd} &middot; " +
            $"{ruta.Estado} &middot; " +
            $"{ruta.DistanciaTotalKm} km &middot; " +
            $"{ruta.TiempoEstimadoMin} min\n" +
            "</header>\n" +
            "<div id=\"mapa\"></div>\n" +
            "<script src=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.js\"></script>\n" +
            "<script>\n" +
            $"  const paradas = {paradasJson};\n" +
            $"  const mapa = L.map('mapa').setView([{centroLatTexto}, {centroLngTexto}], 13);\n" +
            "  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19 }).addTo(mapa);\n" +
            "  const puntos = [];\n" +
            "  paradas.forEach(function (p) {\n" +
            "    const marcador = L.circleMarker([p.lat, p.lng], { radius: 12, color: '#1f2937', fillColor: '#3b82f6', fillOpacity: 0.9 }).addTo(mapa);\n" +
            "    marcador.bindTooltip(String(p.orden), { permanent: true, direction: 'center' });\n" +
            "    marcador.bindPopup('<b>' + p.paciente + '</b><br/>' + p.direccion + '<br/>' + p.estado);\n" +
            "    puntos.push([p.lat, p.lng]);\n" +
            "  });\n" +
            "  if (puntos.length > 1) { L.polyline(puntos, { color: '#3b82f6' }).addTo(mapa); }\n" +
            "</script>\n" +
            "</body>\n" +
            "</html>\n";
    }

    private sealed record MarcadorParada(int Orden, decimal Lat, decimal Lng, string Paciente, string Direccion, string Estado);
}
