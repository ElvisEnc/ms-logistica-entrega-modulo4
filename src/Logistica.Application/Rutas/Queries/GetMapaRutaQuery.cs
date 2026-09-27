using MediatR;

namespace Logistica.Application.Rutas.Queries;

// HU-39 (demo). Devuelve la pagina HTML autocontenida con el recorrido dibujado (§11.3).
public sealed record GetMapaRutaQuery(Guid RutaId) : IRequest<string>;
