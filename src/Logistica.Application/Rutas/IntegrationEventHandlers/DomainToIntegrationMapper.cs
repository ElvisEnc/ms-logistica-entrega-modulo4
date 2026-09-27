using Logistica.Application.IntegrationEvents;
using Logistica.Domain.Rutas.Events;
using Logistica.Domain.Shared;

namespace Logistica.Application.Rutas.IntegrationEventHandlers;

// §8.3 DESIGN.md: aplana los tipos del dominio a primitivos y DTO. pacienteId y
// contratoCateringId viajan SIN TRANSFORMACION (I14): no se resuelven, no se reformatean.
internal static class DomainToIntegrationMapper
{
    public static RutaOptimizadaGeneradaIntegrationEvent ARutaOptimizadaGenerada(RutaOptimizadaGenerada evento) =>
        new(
            evento.RutaId.Value,
            evento.RepartidorId.Value,
            evento.Fecha,
            evento.ParadasOrdenadas.Select(AParadaOrdenadaDto).ToList());

    public static EntregaConfirmadaIntegrationEvent AEntregaConfirmada(EntregaConfirmada evento) =>
        new(
            evento.RutaId.Value,
            evento.ParadaId.Value,
            evento.PaqueteId.Value,
            evento.PacienteId.Value,
            evento.PacienteNombre,
            AConstanciaDto(evento.Constancia));

    public static IncidenciaEntregaRegistradaIntegrationEvent AIncidenciaEntregaRegistrada(IncidenciaEntregaRegistrada evento) =>
        new(
            evento.RutaId.Value,
            evento.ParadaId.Value,
            evento.PaqueteId.Value,
            evento.PacienteId.Value,
            evento.PacienteNombre,
            evento.ContratoCateringId.Value,
            evento.Incidencia.Motivo,
            evento.Incidencia.Descripcion,
            evento.Incidencia.FechaHora,
            evento.Incidencia.UrlFoto);

    private static ParadaOrdenadaDto AParadaOrdenadaDto(ParadaOrdenada parada) =>
        new(
            parada.ParadaId.Value,
            parada.Orden,
            parada.PaqueteId.Value,
            parada.PacienteId.Value,
            parada.PacienteNombre,
            ADireccionGeoDto(parada.DireccionEntrega));

    private static ConstanciaDto AConstanciaDto(ConstanciaEntrega constancia) =>
        new(
            constancia.Tipo,
            constancia.UrlEvidencia,
            constancia.ReceptorNombre,
            constancia.FechaHora,
            // El campo EXISTE en el payload aunque valga null (§9 del maestro)
            constancia.CoordenadasConfirmacion is null ? null : ACoordenadasDto(constancia.CoordenadasConfirmacion));

    private static DireccionGeoDto ADireccionGeoDto(DireccionGeo direccion) =>
        new(direccion.Calle, direccion.Zona, direccion.Ciudad, direccion.Referencia, ACoordenadasDto(direccion.Coordenadas));

    private static CoordenadasDto ACoordenadasDto(Coordenadas coordenadas) =>
        new(coordenadas.Latitud, coordenadas.Longitud);
}
