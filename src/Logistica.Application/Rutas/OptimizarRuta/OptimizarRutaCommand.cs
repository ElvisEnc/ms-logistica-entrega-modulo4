using MediatR;

namespace Logistica.Application.Rutas.OptimizarRuta;

// El origen viaja desplegado en campos primitivos (§10.1 DESIGN.md): el handler construye la
// DireccionGeo y las Coordenadas, de modo que las validaciones de rango ocurren en el DOMINIO.
internal sealed record OptimizarRutaCommand(
    Guid RutaId,
    string Calle,
    string Zona,
    string Ciudad,
    string? Referencia,
    decimal Latitud,
    decimal Longitud) : IRequest;
