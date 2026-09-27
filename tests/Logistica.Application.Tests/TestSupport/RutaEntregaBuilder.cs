using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.TestSupport;

// Builder de datos de prueba (UT-12): el constructor de ParadaEntrega es internal (§5.2
// DESIGN.md, "toda operacion sobre una parada entra por la ruta"), asi que la unica via para
// obtener instancias reales es navegar RutaEntrega.Paradas tras pasar por el factory publico
// RutaEntrega.Crear(...). Espejo (en su propio namespace) del builder homonimo de
// Logistica.Domain.Tests: Application.Tests no depende de Domain.Tests (§6 del MAPA).
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
}
