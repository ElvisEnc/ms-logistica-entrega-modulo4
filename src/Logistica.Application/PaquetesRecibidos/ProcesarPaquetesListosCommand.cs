using Logistica.Application.IntegrationEvents;
using MediatR;

namespace Logistica.Application.PaquetesRecibidos;

internal sealed record ProcesarPaquetesListosCommand(PaquetesListosParaEntrega Contrato) : IRequest;
