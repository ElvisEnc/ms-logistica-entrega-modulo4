using MediatR;

namespace Logistica.Application.Rutas.CompletarRuta;

internal sealed record CompletarRutaCommand(Guid RutaId) : IRequest;
