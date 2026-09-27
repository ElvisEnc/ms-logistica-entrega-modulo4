namespace Logistica.Application.IntegrationEvents;

// SISTEMA.md §8: las coordenadas van ANIDADAS, no aplanadas.
public sealed record DireccionGeoDto(
    string Calle,
    string Zona,
    string Ciudad,
    string? Referencia,
    CoordenadasDto Coordenadas);
