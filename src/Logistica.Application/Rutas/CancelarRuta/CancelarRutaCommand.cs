using MediatR;

namespace Logistica.Application.Rutas.CancelarRuta;

internal sealed record CancelarRutaCommand(Guid RutaId, string Motivo) : IRequest;
