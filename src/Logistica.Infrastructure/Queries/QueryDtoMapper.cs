using Logistica.Application.IntegrationEvents;
using Logistica.Application.PaquetesRecibidos;
using Logistica.Application.PaquetesRecibidos.Queries;
using Logistica.Application.Repartidores.Queries;
using Logistica.Application.Rutas.Queries;
using Logistica.Domain.Repartidores;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;

namespace Logistica.Infrastructure.Queries;

// §10.2 DESIGN.md: el mapeo a DTO se aplica DESPUES de materializar (ToListAsync/
// FirstOrDefaultAsync), nunca dentro de un Select() traducido a SQL: el acceso a .Value de un id
// convertido y enum.ToString() no se traducen a SQL de forma confiable.
internal static class QueryDtoMapper
{
    public static RepartidorDto ToDto(Repartidor repartidor) =>
        new(
            repartidor.RepartidorId.Value,
            repartidor.Nombre,
            repartidor.Telefono,
            repartidor.Vehiculo.Tipo,
            repartidor.Vehiculo.Placa,
            repartidor.Vehiculo.CapacidadPaquetes,
            repartidor.Disponible);

    public static PaqueteRecibidoDto ToDto(PaqueteRecibido paquete) =>
        new(
            paquete.PaqueteId,
            paquete.PacienteId,
            paquete.PacienteNombre,
            ToDireccionDto(paquete.DireccionEntrega),
            paquete.ContratoCateringId,
            ToEtiquetaDto(paquete.Etiqueta),
            paquete.FechaEntrega,
            paquete.Estado);

    public static RutaDetalleDto ToDetalleDto(RutaEntrega ruta) =>
        new(
            ruta.RutaId.Value,
            ruta.RepartidorId.Value,
            ruta.Fecha,
            ruta.Estado,
            EstaOptimizada(ruta), // derivada: el shadow field _optimizada no es legible fuera del DbContext que lo mapeo
            ruta.DistanciaTotalKm,
            ruta.TiempoEstimadoMin,
            ruta.Paradas.OrderBy(p => p.Orden).Select(ToParadaDetalleDto).ToList());

    public static ParadaDetalleDto ToParadaDetalleDto(ParadaEntrega parada) =>
        new(
            parada.ParadaId.Value,
            parada.Orden,
            parada.PaqueteId.Value,
            parada.PacienteId.Value,
            parada.PacienteNombre,
            ToDireccionDto(parada.DireccionEntrega),
            parada.Estado,
            ToConstanciaDto(parada.Constancia),
            ToIncidenciaDto(parada.Incidencia));

    public static EstadoEntregaDto ToEstadoEntregaDto(RutaEntrega ruta, ParadaEntrega parada) =>
        new(
            ruta.RutaId.Value,
            ruta.RepartidorId.Value,
            parada.ParadaId.Value,
            parada.PaqueteId.Value,
            parada.PacienteNombre,
            parada.Estado);

    public static HistorialConstanciaDto ToHistorialDto(RutaEntrega ruta, ParadaEntrega parada) =>
        new(
            ruta.RutaId.Value,
            parada.ParadaId.Value,
            parada.PaqueteId.Value,
            parada.PacienteId.Value,
            parada.PacienteNombre,
            ruta.Fecha,
            parada.Estado,
            ToConstanciaDto(parada.Constancia),
            ToIncidenciaDto(parada.Incidencia));

    // "Hay paradas y todas tienen Orden >= 1" es cierto exactamente cuando Optimizar corrio
    // (§10.2 DESIGN.md). Es informacion de presentacion; la garantia de I3 sigue viniendo del
    // campo persistido, que si se lee al materializar el agregado para un comando.
    private static bool EstaOptimizada(RutaEntrega ruta) =>
        ruta.Paradas.Count > 0 && ruta.Paradas.All(p => p.Orden >= 1);

    private static DireccionGeoDto ToDireccionDto(DireccionGeo direccion) =>
        new(
            direccion.Calle,
            direccion.Zona,
            direccion.Ciudad,
            direccion.Referencia,
            new CoordenadasDto(direccion.Coordenadas.Latitud, direccion.Coordenadas.Longitud));

    private static EtiquetaDto ToEtiquetaDto(EtiquetaPaquete etiqueta) =>
        new(
            etiqueta.PaqueteId,
            etiqueta.NombrePaciente,
            etiqueta.NroIdentificacion,
            ToDireccionDto(etiqueta.DireccionEntrega),
            etiqueta.Fecha,
            etiqueta.CodigoQR);

    private static ConstanciaEntregaDto? ToConstanciaDto(ConstanciaEntrega? constancia) =>
        constancia is null
            ? null
            : new ConstanciaEntregaDto(
                constancia.Tipo,
                constancia.UrlEvidencia,
                constancia.ReceptorNombre,
                constancia.FechaHora,
                constancia.CoordenadasConfirmacion is null
                    ? null
                    : new CoordenadasDto(constancia.CoordenadasConfirmacion.Latitud, constancia.CoordenadasConfirmacion.Longitud));

    private static IncidenciaEntregaDto? ToIncidenciaDto(IncidenciaEntrega? incidencia) =>
        incidencia is null
            ? null
            : new IncidenciaEntregaDto(incidencia.Motivo, incidencia.Descripcion, incidencia.FechaHora, incidencia.UrlFoto);
}
