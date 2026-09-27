using Joseco.DDD.Core.Results;
using Logistica.Domain.Rutas;
using Logistica.Domain.Rutas.Events;
using Logistica.Domain.Shared;
using Logistica.Domain.Tests.TestSupport;

namespace Logistica.Domain.Tests.Rutas;

// Cubre §4.1 (I8, filas 21-22) y §4.2 "RutaEntregaOptimizacionTests" del MAPA de unit tests:
// RutaEntrega.Optimizar (vecino mas cercano sobre distancia haversine, RN-15/HU-38/§12.4) y el
// guard de I8 (RN-13, RN-15) que valida que los ordenes asignados formen 1..n sin repetir.
[Trait("Capa", "Unit")]
public class RutaEntregaOptimizacionTests
{
    // Direccion generica con coordenadas elegidas a mano (misma longitud, latitudes con paso
    // constante) para que la distancia esperada se pueda verificar por calculo: con longitud
    // igual, el haversine colapsa a R * radianes(deltaLatitud), sin termino cruzado.
    private static DireccionGeo DireccionEn(decimal latitud, decimal longitud) =>
        DireccionGeo.Crear("Calle de prueba", "Zona de prueba", "Santa Cruz de la Sierra", null,
            Coordenadas.Crear(latitud, longitud));

    // Direccion con datos reales de Santa Cruz de la Sierra, Bolivia: la zona se usa como
    // identificador legible para verificar el orden resultante en el test de coordenadas reales.
    private static DireccionGeo DireccionSantaCruz(string calle, string zona, decimal latitud, decimal longitud) =>
        DireccionGeo.Crear(calle, zona, "Santa Cruz de la Sierra", null, Coordenadas.Crear(latitud, longitud));

    private static PaqueteParaRuta PaqueteEn(DireccionGeo direccion) =>
        PaqueteParaRuta.Crear(PaqueteId.New(), PacienteId.New(), "Juan Perez", direccion, ContratoId.New());

    [Fact]
    public void Optimizar_con_tres_paradas_asigna_ordenes_1_a_n_sin_repetir_I8()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.Pendiente(3);
        var origen = RutaEntregaBuilder.Origen();

        // Act
        ruta.Optimizar(origen); // I8 (RN-13, RN-15)

        // Assert
        var ordenes = ruta.Paradas.Select(p => p.Orden).OrderBy(o => o).ToList();
        Assert.Equal(new List<int> { 1, 2, 3 }, ordenes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AsignarOrden_menor_a_uno_lanza_I8_ORDEN_PARADAS_INVALIDO(int ordenInvalido)
    {
        // Arrange
        var ruta = RutaEntregaBuilder.Pendiente();
        var parada = ruta.Paradas.Single();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            parada.AsignarOrden(ordenInvalido)); // I8 (RN-13, RN-15)

        // Assert
        Assert.Equal("I8_ORDEN_PARADAS_INVALIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Optimizar_calcula_distancia_y_tiempo_estimado_correctos()
    {
        // Arrange
        // Tres direcciones sobre la misma longitud, a 0.1 grado de latitud de distancia entre
        // consecutivas: el orden de vecino mas cercano desde el origen es, por construccion,
        // A -> B -> C (monotono en distancia). Se cargan en el orden C, A, B para probar que el
        // algoritmo reordena y no se limita a conservar el orden de entrada.
        var origen = DireccionEn(-17.0m, -63.0m);
        var direccionA = DireccionEn(-17.1m, -63.0m);
        var direccionB = DireccionEn(-17.2m, -63.0m);
        var direccionC = DireccionEn(-17.3m, -63.0m);
        var paqueteA = PaqueteEn(direccionA);
        var paqueteB = PaqueteEn(direccionB);
        var paqueteC = PaqueteEn(direccionC);
        var ruta = RutaEntrega.Crear(RutaEntregaBuilder.FechaPorDefecto, RepartidorId.New(), new[] { paqueteC, paqueteA, paqueteB });

        // Act
        ruta.Optimizar(origen); // I3, I8 (RN-13, RN-15)

        // Assert
        // Distancia esperada: se reutiliza DireccionGeo.DistanciaHasta (formula real, ya cubierta
        // por DireccionGeoTests) sobre el orden esperado origen->A->B->C, para no reinventar el
        // haversine a mano y en cambio verificar la suma/orquestacion que hace Optimizar.
        var distanciaEsperada = origen.DistanciaHasta(direccionA)
            + direccionA.DistanciaHasta(direccionB)
            + direccionB.DistanciaHasta(direccionC);
        Assert.Equal(distanciaEsperada, ruta.DistanciaTotalKm);

        // VelocidadPromedioKmh = 30m y MinutosPorParada = 5 son constantes privadas de
        // RutaEntrega.cs (leidas del codigo real, no inventadas).
        var tiempoEsperado = (int)Math.Round(distanciaEsperada / 30m * 60m + 5 * ruta.Paradas.Count);
        Assert.Equal(tiempoEsperado, ruta.TiempoEstimadoMin);
    }

    [Fact]
    public void Optimizar_emite_RutaOptimizadaGenerada_con_las_paradas_ordenadas()
    {
        // Arrange
        var repartidorId = RepartidorId.New();
        var ruta = RutaEntregaBuilder.Pendiente(3, repartidorId: repartidorId);
        var origen = RutaEntregaBuilder.Origen();

        // Act
        ruta.Optimizar(origen);

        // Assert
        var evento = Assert.Single(ruta.DomainEvents.OfType<RutaOptimizadaGenerada>());
        Assert.Equal(ruta.RutaId, evento.RutaId);
        Assert.Equal(repartidorId, evento.RepartidorId);
        Assert.Equal(ruta.Fecha, evento.Fecha);

        var paradasEsperadas = ruta.Paradas
            .OrderBy(p => p.Orden)
            .Select(p => (p.ParadaId, p.Orden, p.PaqueteId, p.PacienteId, p.PacienteNombre, p.DireccionEntrega))
            .ToList();
        var paradasDelEvento = evento.ParadasOrdenadas
            .Select(p => (p.ParadaId, p.Orden, p.PaqueteId, p.PacienteId, p.PacienteNombre, p.DireccionEntrega))
            .ToList();
        Assert.Equal(paradasEsperadas, paradasDelEvento);
    }

    [Fact]
    public void Optimizar_ejecutado_dos_veces_recalcula_desde_cero()
    {
        // Arrange
        var origen = DireccionEn(-17.0m, -63.0m);
        var paqueteA = PaqueteEn(DireccionEn(-17.1m, -63.0m));
        var paqueteB = PaqueteEn(DireccionEn(-17.2m, -63.0m));
        var paqueteC = PaqueteEn(DireccionEn(-17.3m, -63.0m));
        var ruta = RutaEntrega.Crear(RutaEntregaBuilder.FechaPorDefecto, RepartidorId.New(), new[] { paqueteC, paqueteA, paqueteB });

        // Act
        ruta.Optimizar(origen);
        var distanciaPrimeraVez = ruta.DistanciaTotalKm;
        var tiempoPrimeraVez = ruta.TiempoEstimadoMin;
        var ordenesPrimeraVez = ruta.Paradas.OrderBy(p => p.ParadaId.Value).Select(p => p.Orden).ToList();

        ruta.Optimizar(origen); // segunda vez, mismo origen: si acumulara estado, distancia/tiempo cambiarian

        // Assert
        Assert.Equal(distanciaPrimeraVez, ruta.DistanciaTotalKm);
        Assert.Equal(tiempoPrimeraVez, ruta.TiempoEstimadoMin);
        var ordenesSegundaVez = ruta.Paradas.OrderBy(p => p.ParadaId.Value).Select(p => p.Orden).ToList();
        Assert.Equal(ordenesPrimeraVez, ordenesSegundaVez);
        Assert.Equal(2, ruta.DomainEvents.OfType<RutaOptimizadaGenerada>().Count());
    }

    [Fact]
    public void Optimizar_con_paradas_reales_de_Santa_Cruz_produce_el_orden_esperado()
    {
        // Arrange
        // Coordenadas reales de Santa Cruz de la Sierra, Bolivia. El orden de vecino mas cercano
        // se calculo por fuera (script con la misma formula haversine, R = 6371 km) antes de
        // escribir este assert: desde la Plaza 24 de Septiembre, Equipetrol Norte queda a ~2.89 km,
        // Urubo Oeste a ~6.28 km y Villa 1ro de Mayo Sur a ~7.46 km; desde Equipetrol Norte, Urubo
        // Oeste queda a ~6.45 km y Villa 1ro de Mayo Sur a ~10.23 km. Los margenes entre candidatos
        // son de kilometros, muy por encima de cualquier diferencia de precision numerica.
        var origen = DireccionSantaCruz("Calle Junin", "Casco Viejo", -17.783312m, -63.182097m); // Plaza 24 de Septiembre
        var direccionEquipetrol = DireccionSantaCruz("Av. San Martin", "Equipetrol Norte", -17.760000m, -63.170000m);
        var direccionUrubo = DireccionSantaCruz("Av. Roca y Coronado", "Urubo Oeste", -17.750000m, -63.230000m);
        var direccionVilla1roMayo = DireccionSantaCruz("Av. Grigota", "Villa 1ro de Mayo Sur", -17.850000m, -63.190000m);

        var paqueteEquipetrol = PaqueteEn(direccionEquipetrol);
        var paqueteUrubo = PaqueteEn(direccionUrubo);
        var paqueteVilla1roMayo = PaqueteEn(direccionVilla1roMayo);
        var ruta = RutaEntrega.Crear(
            RutaEntregaBuilder.FechaPorDefecto,
            RepartidorId.New(),
            new[] { paqueteVilla1roMayo, paqueteEquipetrol, paqueteUrubo }); // orden de entrada distinto al esperado

        // Act
        ruta.Optimizar(origen);

        // Assert
        var zonasEnOrden = ruta.Paradas.OrderBy(p => p.Orden).Select(p => p.DireccionEntrega.Zona).ToList();
        Assert.Equal(new[] { "Equipetrol Norte", "Urubo Oeste", "Villa 1ro de Mayo Sur" }, zonasEnOrden);

        var distanciaEsperada = origen.DistanciaHasta(direccionEquipetrol)
            + direccionEquipetrol.DistanciaHasta(direccionUrubo)
            + direccionUrubo.DistanciaHasta(direccionVilla1roMayo);
        Assert.Equal(distanciaEsperada, ruta.DistanciaTotalKm);
    }
}
