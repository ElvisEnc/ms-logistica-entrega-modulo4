using MediatR;

namespace Logistica.Application.Rutas.IniciarRuta;

internal sealed record IniciarRutaCommand(Guid RutaId) : IRequest;
