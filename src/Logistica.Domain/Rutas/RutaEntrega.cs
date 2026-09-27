using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Logistica.Domain.Rutas.Events;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Rutas;

public sealed class RutaEntrega : AggregateRoot
{
    private const decimal VelocidadPromedioKmh = 30m;
    private const int MinutosPorParada = 5;

    private readonly List<ParadaEntrega> _paradas = new();
    private bool _optimizada;

    public RutaId RutaId => RutaId.From(Id);
    public DateOnly Fecha { get; private set; }
    public RepartidorId RepartidorId { get; private set; } = null!;
    public EstadoRuta Estado { get; private set; }
    public decimal DistanciaTotalKm { get; private set; }
    public int TiempoEstimadoMin { get; private set; }
    public IReadOnlyCollection<ParadaEntrega> Paradas => _paradas.AsReadOnly();

    private RutaEntrega() { } // EF Core

    private RutaEntrega(Guid id, DateOnly fecha, RepartidorId repartidorId) : base(id)
    {
        Fecha = fecha;
        RepartidorId = repartidorId;
        Estado = EstadoRuta.PENDIENTE;
    }

    public static RutaEntrega Crear(DateOnly fecha, RepartidorId repartidorId, IReadOnlyCollection<PaqueteParaRuta> paquetes)
    {
        if (paquetes is null || paquetes.Count == 0)
            throw new DomainException(RutaErrors.SinParadas()); // I1 (RN-13)

        var ruta = new RutaEntrega(Guid.NewGuid(), fecha, repartidorId);

        foreach (var paquete in paquetes)
            ruta._paradas.Add(new ParadaEntrega(paquete)); // I14 (RN-16, RN-23) estructural: cada parada nace con PacienteId y ContratoCateringId

        ruta.AddDomainEvent(new RutaCreada(
            ruta.RutaId,
            ruta.RepartidorId,
            ruta.Fecha,
            ruta._paradas.Select(p => p.PaqueteId).ToList()));

        return ruta;
    }

    // RN-15, HU-38, §12.4: vecino mas cercano sobre distancia haversine.
    //
    // Pseudocodigo de §12.4 (transcrito antes de escribir C#, §17 fase 2):
    //
    //   requiere: Estado == PENDIENTE                                    // I3
    //
    //   actual         <- origen                   (DireccionGeo del comando)
    //   pendientes     <- copia de _paradas
    //   orden          <- 1
    //   distanciaTotal <- 0
    //
    //   mientras pendientes no este vacio:
    //       p <- la parada de pendientes con menor actual.DistanciaHasta(p.DireccionEntrega)
    //       p.AsignarOrden(orden)                                        // rechaza orden < 1
    //       distanciaTotal += actual.DistanciaHasta(p.DireccionEntrega)
    //       actual <- p.DireccionEntrega
    //       orden  += 1
    //       quitar p de pendientes
    //
    //   verificar que los ordenes asignados son exactamente 1..n sin repetir   // guard de I8
    //
    //   DistanciaTotalKm  <- distanciaTotal
    //   TiempoEstimadoMin <- redondear( distanciaTotal / VELOCIDAD_PROMEDIO_KMH * 60
    //                                   + MINUTOS_POR_PARADA * cantidadDeParadas )
    //   _optimizada <- true                                               // sostiene I3
    //   emitir RutaOptimizadaGenerada con las paradas ordenadas por Orden
    //
    // El C# de abajo sigue este pseudocodigo paso a paso, sin desviaciones.
    // D-02 (§19.1): el punto de partida es el `origen` EXPLICITO del parametro,
    // nunca `_paradas` (o su copia) en el orden en que llego el lote de BC5 —
    // eso es lo que NUR-TRICENTER hacia mal y esta especificacion corrige.
    public void Optimizar(DireccionGeo origen)
    {
        if (Estado != EstadoRuta.PENDIENTE)
            throw new DomainException(RutaErrors.NoPendiente()); // I3 (RN-13, RN-15)

        var actual = origen;
        var pendientes = new List<ParadaEntrega>(_paradas);
        var orden = 1;
        var distanciaTotal = 0m;

        while (pendientes.Count > 0)
        {
            var siguiente = pendientes.OrderBy(p => actual.DistanciaHasta(p.DireccionEntrega)).First();

            siguiente.AsignarOrden(orden);
            distanciaTotal += actual.DistanciaHasta(siguiente.DireccionEntrega);
            actual = siguiente.DireccionEntrega;
            orden++;
            pendientes.Remove(siguiente);
        }

        // I8 (RN-13, RN-15): guard explicito, el orden no puede ser solo una propiedad emergente del bucle
        var ordenesAsignados = _paradas.Select(p => p.Orden).OrderBy(o => o);
        if (!ordenesAsignados.SequenceEqual(Enumerable.Range(1, _paradas.Count)))
            throw new DomainException(RutaErrors.OrdenParadasInvalido()); // I8 (RN-13, RN-15)

        DistanciaTotalKm = distanciaTotal;
        TiempoEstimadoMin = (int)Math.Round(distanciaTotal / VelocidadPromedioKmh * 60m + MinutosPorParada * _paradas.Count);
        _optimizada = true;

        var paradasOrdenadas = _paradas
            .OrderBy(p => p.Orden)
            .Select(p => new ParadaOrdenada(p.ParadaId, p.Orden, p.PaqueteId, p.PacienteId, p.PacienteNombre, p.DireccionEntrega))
            .ToList();

        AddDomainEvent(new RutaOptimizadaGenerada(RutaId, RepartidorId, Fecha, paradasOrdenadas));
    }

    public void Iniciar()
    {
        if (Estado != EstadoRuta.PENDIENTE)
            throw new DomainException(RutaErrors.NoPendiente()); // I3 (RN-13, RN-15)

        if (!_optimizada)
            throw new DomainException(RutaErrors.NoOptimizada()); // I3 (RN-13, RN-15)

        foreach (var parada in _paradas.Where(p => p.Estado == EstadoEntrega.PENDIENTE))
            parada.MarcarEnCamino();

        Estado = EstadoRuta.EN_CAMINO;

        AddDomainEvent(new RutaIniciada(RutaId, RepartidorId));
    }

    public void ConfirmarEntrega(ParadaId paradaId, ConstanciaEntrega constancia)
    {
        if (Estado != EstadoRuta.EN_CAMINO)
            throw new DomainException(RutaErrors.TransicionInvalida()); // I4 (RN-16)

        var parada = BuscarParada(paradaId);
        parada.ConfirmarEntrega(constancia); // I4 (RN-16) nivel parada, I5 (RN-16)

        AddDomainEvent(new EntregaConfirmada(
            RutaId, parada.ParadaId, parada.PaqueteId, parada.PacienteId, parada.PacienteNombre, constancia)); // I14 (RN-16, RN-23)
    }

    public void ReportarIncidencia(ParadaId paradaId, IncidenciaEntrega incidencia)
    {
        if (Estado != EstadoRuta.EN_CAMINO)
            throw new DomainException(RutaErrors.TransicionInvalida()); // I4 (RN-16)

        var parada = BuscarParada(paradaId);
        parada.ReportarIncidencia(incidencia); // I4 (RN-16) nivel parada, I5 (RN-16)

        AddDomainEvent(new IncidenciaEntregaRegistrada(
            RutaId, parada.ParadaId, parada.PaqueteId, parada.PacienteId, parada.PacienteNombre,
            parada.ContratoCateringId, incidencia)); // I14 (RN-16, RN-23): ContratoCateringId leido de la parada
    }

    public void Completar()
    {
        if (_paradas.Any(p => !p.EstaResuelta()))
            throw new DomainException(RutaErrors.ParadasPendientes()); // I6 (RN-16)

        Estado = _paradas.Any(p => p.Estado == EstadoEntrega.NO_ENTREGADO)
            ? EstadoRuta.CON_INCIDENCIAS
            : EstadoRuta.COMPLETADA; // I7 (RN-16): lo determina el desenlace, no quien llama

        AddDomainEvent(new RutaCompletada(RutaId, RepartidorId, Estado));
    }

    public void Cancelar(string motivo)
    {
        if (Estado is not (EstadoRuta.PENDIENTE or EstadoRuta.EN_CAMINO))
            throw new DomainException(RutaErrors.NoCancelable()); // I13 (RN-13)

        if (string.IsNullOrWhiteSpace(motivo))
            throw new DomainException(RutaErrors.MotivoRequerido()); // I13 (RN-13)

        // las paradas ya resueltas no se tocan: su constancia/incidencia es evidencia historica (RN-16)
        var paqueteIdsNoResueltos = _paradas.Where(p => !p.EstaResuelta()).Select(p => p.PaqueteId).ToList();

        Estado = EstadoRuta.CANCELADA;

        AddDomainEvent(new RutaCancelada(RutaId, RepartidorId, motivo, paqueteIdsNoResueltos));
    }

    public ParadaEntrega? ObtenerSiguienteParada() =>
        _paradas.Where(p => !p.EstaResuelta()).OrderBy(p => p.Orden).FirstOrDefault();

    private ParadaEntrega BuscarParada(ParadaId paradaId) =>
        _paradas.FirstOrDefault(p => p.ParadaId == paradaId)
        ?? throw new DomainException(RutaErrors.ParadaNoEncontrada()); // precondicion del comando, nunca NotFound
}
