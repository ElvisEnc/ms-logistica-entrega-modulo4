namespace Logistica.Application.IntegrationEvents;

// SISTEMA.md §8: tipo compartido, forma identica en los seis microservicios.
public sealed record CoordenadasDto(decimal Latitud, decimal Longitud);
