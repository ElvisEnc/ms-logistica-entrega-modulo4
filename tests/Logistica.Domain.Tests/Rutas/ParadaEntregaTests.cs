using Joseco.DDD.Core.Results;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;
using Logistica.Domain.Tests.TestSupport;

namespace Logistica.Domain.Tests.Rutas;

[Trait("Capa", "Unit")]
public class ParadaEntregaTests
{
    // Describe uno de los cuatro estados de EstadoEntrega para EstaResuelta_...: se parametriza en
    // vez de repetir el mismo Theory body con un if/switch (UT-12), igual que DescriptorTypedId en
    // TypedIdTests.
    public sealed class EscenarioEstaResuelta
    {
        public required string Nombre { get; init; }
        public required Func<ParadaEntrega> ConstruirParada { get; init; }
        public required bool EstaResueltaEsperado { get; init; }

        public override string ToString() => Nombre;
    }

    public static TheoryData<EscenarioEstaResuelta> Escenarios() => new()
    {
        new EscenarioEstaResuelta
        {
            Nombre = "PENDIENTE",
            ConstruirParada = () => RutaEntregaBuilder.Pendiente().Paradas.Single(),
            EstaResueltaEsperado = false,
        },
        new EscenarioEstaResuelta
        {
            Nombre = "EN_CAMINO",
            ConstruirParada = () => RutaEntregaBuilder.EnCamino().Paradas.Single(),
            EstaResueltaEsperado = false,
        },
        new EscenarioEstaResuelta
        {
            Nombre = "ENTREGADO",
            ConstruirParada = ParadaEntregada,
            EstaResueltaEsperado = true,
        },
        new EscenarioEstaResuelta
        {
            Nombre = "NO_ENTREGADO",
            ConstruirParada = ParadaNoEntregada,
            EstaResueltaEsperado = true,
        },
    };

    private static ParadaEntrega ParadaEntregada()
    {
        var ruta = RutaEntregaBuilder.EnCamino();
        var parada = ruta.Paradas.Single();
        parada.ConfirmarEntrega(RutaEntregaBuilder.ConstanciaValida());
        return parada;
    }

    private static ParadaEntrega ParadaNoEntregada()
    {
        var ruta = RutaEntregaBuilder.EnCamino();
        var parada = ruta.Paradas.Single();
        parada.ReportarIncidencia(RutaEntregaBuilder.IncidenciaValida());
        return parada;
    }

    [Fact]
    public void ConfirmarEntrega_sobre_ruta_no_en_camino_lanza_I4_TRANSICION_INVALIDA()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.Pendiente();
        var parada = ruta.Paradas.Single();
        var constancia = RutaEntregaBuilder.ConstanciaValida();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            ruta.ConfirmarEntrega(parada.ParadaId, constancia)); // I4 (RN-16)

        // Assert
        Assert.Equal("I4_TRANSICION_INVALIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
    }

    [Fact]
    public void ReportarIncidencia_sobre_ruta_no_en_camino_lanza_I4_TRANSICION_INVALIDA()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.Pendiente();
        var parada = ruta.Paradas.Single();
        var incidencia = RutaEntregaBuilder.IncidenciaValida();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            ruta.ReportarIncidencia(parada.ParadaId, incidencia)); // I4 (RN-16)

        // Assert
        Assert.Equal("I4_TRANSICION_INVALIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
    }

    [Fact]
    public void ConfirmarEntrega_sobre_parada_ya_resuelta_lanza_I4_TRANSICION_INVALIDA()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var parada = ruta.Paradas.Single();
        ruta.ConfirmarEntrega(parada.ParadaId, RutaEntregaBuilder.ConstanciaValida());

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            ruta.ConfirmarEntrega(parada.ParadaId, RutaEntregaBuilder.ConstanciaValida())); // I4 (RN-16)

        // Assert
        Assert.Equal("I4_TRANSICION_INVALIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
    }

    // Simetrico de ConfirmarEntrega_sobre_parada_ya_resuelta_lanza_I4_TRANSICION_INVALIDA (Derivado,
    // MAPA §4.2): la guarda EstaResuelta() de ParadaEntrega.ReportarIncidencia no tenia test propio.
    [Fact]
    public void ReportarIncidencia_sobre_parada_ya_resuelta_lanza_I4_TRANSICION_INVALIDA()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var parada = ruta.Paradas.Single();
        ruta.ReportarIncidencia(parada.ParadaId, RutaEntregaBuilder.IncidenciaValida());

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            ruta.ReportarIncidencia(parada.ParadaId, RutaEntregaBuilder.IncidenciaValida())); // I4 (RN-16)

        // Assert
        Assert.Equal("I4_TRANSICION_INVALIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Conflict, excepcion.Error.Type);
    }

    [Fact]
    public void ConfirmarEntrega_con_constancia_pasa_a_ENTREGADO_y_deja_incidencia_nula()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var parada = ruta.Paradas.Single();
        var constancia = RutaEntregaBuilder.ConstanciaValida();

        // Act
        parada.ConfirmarEntrega(constancia); // I5 (RN-16)

        // Assert
        Assert.Equal(EstadoEntrega.ENTREGADO, parada.Estado);
        Assert.Equal(constancia, parada.Constancia);
        Assert.Null(parada.Incidencia);
    }

    [Fact]
    public void ReportarIncidencia_con_incidencia_pasa_a_NO_ENTREGADO_y_deja_constancia_nula()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var parada = ruta.Paradas.Single();
        var incidencia = RutaEntregaBuilder.IncidenciaValida();

        // Act
        parada.ReportarIncidencia(incidencia); // I5 (RN-16)

        // Assert
        Assert.Equal(EstadoEntrega.NO_ENTREGADO, parada.Estado);
        Assert.Equal(incidencia, parada.Incidencia);
        Assert.Null(parada.Constancia);
    }

    // A diferencia de ConfirmarEntrega_con_constancia_pasa_a_ENTREGADO_..., que llama a
    // ParadaEntrega.ConfirmarEntrega directamente, este ejercita la garantia de I5 pasando por la
    // ruta agregado (RutaEntrega.ConfirmarEntrega), el camino real que usa el handler.
    [Fact]
    public void ConfirmarEntrega_sobre_ruta_en_camino_deja_Incidencia_en_null_I5()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var parada = ruta.Paradas.Single();
        var constancia = RutaEntregaBuilder.ConstanciaValida();

        // Act
        ruta.ConfirmarEntrega(parada.ParadaId, constancia); // I5 (RN-16)

        // Assert
        Assert.Null(parada.Incidencia);
    }

    [Fact]
    public void ReportarIncidencia_sobre_ruta_en_camino_deja_Constancia_en_null_I5()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var parada = ruta.Paradas.Single();
        var incidencia = RutaEntregaBuilder.IncidenciaValida();

        // Act
        ruta.ReportarIncidencia(parada.ParadaId, incidencia); // I5 (RN-16)

        // Assert
        Assert.Null(parada.Constancia);
    }

    [Fact]
    public void ConfirmarEntrega_con_constancia_nula_lanza_I5_CONSTANCIA_REQUERIDA()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var parada = ruta.Paradas.Single();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            parada.ConfirmarEntrega(null!)); // I5 (RN-16)

        // Assert
        Assert.Equal("I5_CONSTANCIA_REQUERIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void ReportarIncidencia_con_incidencia_nula_lanza_I5_INCIDENCIA_REQUERIDA()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.EnCamino();
        var parada = ruta.Paradas.Single();

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            parada.ReportarIncidencia(null!)); // I5 (RN-16)

        // Assert
        Assert.Equal("I5_INCIDENCIA_REQUERIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Nace_pendiente_con_orden_cero_y_direccion_clonada()
    {
        // Arrange
        var direccionOriginal = DireccionGeoBuilder.Valida();
        var paquete = PaqueteParaRuta.Crear(
            PaqueteId.New(), PacienteId.New(), "Juan Perez", direccionOriginal, ContratoId.New());

        // Act
        var ruta = RutaEntrega.Crear(RutaEntregaBuilder.FechaPorDefecto, RepartidorId.New(), new[] { paquete });
        var parada = ruta.Paradas.Single();

        // Assert
        Assert.Equal(EstadoEntrega.PENDIENTE, parada.Estado);
        Assert.Equal(0, parada.Orden);
        Assert.NotSame(direccionOriginal, parada.DireccionEntrega); // segunda clonacion, §5.4
        Assert.Equal(direccionOriginal, parada.DireccionEntrega); // misma direccion en valor
    }

    [Fact]
    public void MarcarEnCamino_pasa_de_PENDIENTE_a_EN_CAMINO()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.Pendiente();
        var parada = ruta.Paradas.Single();

        // Act
        parada.MarcarEnCamino();

        // Assert
        Assert.Equal(EstadoEntrega.EN_CAMINO, parada.Estado);
    }

    // Positivo de I8 (RN-13, RN-15): el negativo (`AsignarOrden_menor_a_uno_lanza_...`) esta fuera
    // del alcance de esta tarea (MAPA §4.1, fila 22).
    [Fact]
    public void AsignarOrden_con_valor_valido_fija_Orden()
    {
        // Arrange
        var ruta = RutaEntregaBuilder.Pendiente();
        var parada = ruta.Paradas.Single();

        // Act
        parada.AsignarOrden(3); // I8 (RN-13, RN-15)

        // Assert
        Assert.Equal(3, parada.Orden);
    }

    [Theory]
    [MemberData(nameof(Escenarios))]
    public void EstaResuelta_es_false_en_PENDIENTE_o_EN_CAMINO_y_true_en_ENTREGADO_o_NO_ENTREGADO(EscenarioEstaResuelta escenario)
    {
        // Arrange
        var parada = escenario.ConstruirParada();

        // Act
        var resultado = parada.EstaResuelta();

        // Assert
        Assert.Equal(escenario.EstaResueltaEsperado, resultado);
    }
}
