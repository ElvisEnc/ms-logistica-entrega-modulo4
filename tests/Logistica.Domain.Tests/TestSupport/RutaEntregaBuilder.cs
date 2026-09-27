using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Tests.TestSupport;

// Builder de datos de prueba (UT-12): el constructor de ParadaEntrega es internal (§5.2
// DESIGN.md, "toda operacion sobre una parada entra por la ruta"), asi que la unica via para
// obtener instancias reales es navegar RutaEntrega.Paradas tras pasar por el factory publico
// RutaEntrega.Crear(...). Se reutiliza en RutaEntregaInvariantesTests y en
// RutaEntregaOptimizacionTests: por eso expone los tres estados tipicos (Pendiente, Optimizada,
// EnCamino) en vez de solo lo minimo para ParadaEntregaTests.
public static class RutaEntregaBuilder
{
    public static readonly DateOnly FechaPorDefecto = new(2026, 9, 27);

    public static DireccionGeo Origen() => DireccionGeoBuilder.Valida();

    public static PaqueteParaRuta Paquete() =>
        PaqueteParaRuta.Crear(
            PaqueteId.New(),
            PacienteId.New(),
            "Juan Perez",
            DireccionGeoBuilder.Valida(),
            ContratoId.New());

    // Recien creada por Crear(): Estado == PENDIENTE, paradas PENDIENTE con Orden == 0 (I1).
    public static RutaEntrega Pendiente(int cantidadParadas = 1, RepartidorId? repartidorId = null, DateOnly? fecha = null)
    {
        var paquetes = Enumerable.Range(0, cantidadParadas).Select(_ => Paquete()).ToList();

        return RutaEntrega.Crear(fecha ?? FechaPorDefecto, repartidorId ?? RepartidorId.New(), paquetes);
    }

    // Pendiente() + Optimizar(origen): Estado sigue PENDIENTE, pero ya admite Iniciar() (I3).
    public static RutaEntrega Optimizada(int cantidadParadas = 1, DireccionGeo? origen = null)
    {
        var ruta = Pendiente(cantidadParadas);
        ruta.Optimizar(origen ?? Origen());
        return ruta;
    }

    // Optimizada() + Iniciar(): Estado == EN_CAMINO, paradas EN_CAMINO (precondicion de I4).
    public static RutaEntrega EnCamino(int cantidadParadas = 1, DireccionGeo? origen = null)
    {
        var ruta = Optimizada(cantidadParadas, origen);
        ruta.Iniciar();
        return ruta;
    }

    // EnCamino() + ConfirmarEntrega en todas las paradas: precondicion del positivo de I6/I7
    // (Completar da COMPLETADA cuando ninguna parada quedo NO_ENTREGADO).
    public static RutaEntrega EnCaminoConTodasEntregadas(int cantidadParadas = 2)
    {
        var ruta = EnCamino(cantidadParadas);

        foreach (var parada in ruta.Paradas.ToList())
            ruta.ConfirmarEntrega(parada.ParadaId, ConstanciaValida());

        return ruta;
    }

    // EnCamino() con una parada en NO_ENTREGADO y el resto ENTREGADO: precondicion de la rama
    // CON_INCIDENCIAS de I7 (Completar_con_una_NO_ENTREGADO_da_CON_INCIDENCIAS_I7).
    public static RutaEntrega EnCaminoConUnaIncidencia(int cantidadParadas = 2)
    {
        var ruta = EnCamino(cantidadParadas);
        var paradas = ruta.Paradas.ToList();

        ruta.ReportarIncidencia(paradas[0].ParadaId, IncidenciaValida());
        foreach (var parada in paradas.Skip(1))
            ruta.ConfirmarEntrega(parada.ParadaId, ConstanciaValida());

        return ruta;
    }

    public static ConstanciaEntrega ConstanciaValida() =>
        ConstanciaEntrega.Crear(
            new DateTime(2026, 9, 27, 14, 0, 0, DateTimeKind.Utc),
            TipoConstancia.FOTO,
            "https://storage.local/evidencias/foto1.jpg",
            "Juan Perez",
            null);

    public static IncidenciaEntrega IncidenciaValida() =>
        IncidenciaEntrega.Crear(
            new DateTime(2026, 9, 27, 14, 0, 0, DateTimeKind.Utc),
            MotivoIncidencia.PACIENTE_AUSENTE,
            "El paciente no se encontraba en el domicilio",
            null);
}
