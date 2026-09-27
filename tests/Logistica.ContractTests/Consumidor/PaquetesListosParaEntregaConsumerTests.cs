using System.Text.Json;
using System.Text.Json.Serialization;
using Logistica.Application.Abstractions;
using Logistica.Application.IntegrationEvents;
using Logistica.Application.PaquetesRecibidos;
using Logistica.ContractTests.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PactNet;
using PactNet.Output.Xunit;
using Xunit.Abstractions;

namespace Logistica.ContractTests.Consumidor;

// Par #9 (SISTEMA.md §13.5): ms-logistica-entrega es CONSUMIDOR de ms-produccion-alimentos.
// Transporte: message pact real de PactNet 5 (sin bloqueo, MAPA-CONTRACT-TESTS.md fila 1-2).
[Trait("Capa", "Contrato")]
public sealed class PaquetesListosParaEntregaConsumerTests
{
    // CT-04/CT-05: mismas JsonSerializerOptions que el binding real del controller
    // (src/Logistica.WebApi/Program.cs, AddJsonOptions).
    private static readonly JsonSerializerOptions ContratoJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ITestOutputHelper _output;

    public PaquetesListosParaEntregaConsumerTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task ConCamposOpcionalesPresentes_SeProcesaPorElHandlerRealYQuedaPorAsignar()
    {
        var contrato = ConstruirContrato(conOpcionales: true);

        await VerificarInteraccion(
            "un lote de paquetes listos con referencia y codigo QR presentes",
            "BC5 etiqueto paquetes con todos los campos opcionales",
            contrato,
            esperaOpcionalesPresentes: true);
    }

    [Fact]
    public async Task ConCamposOpcionalesEnNull_SeProcesaPorElHandlerRealYQuedaPorAsignar()
    {
        var contrato = ConstruirContrato(conOpcionales: false);

        await VerificarInteraccion(
            "un lote de paquetes listos sin referencia ni codigo QR",
            "BC5 etiqueto paquetes sin los campos opcionales",
            contrato,
            esperaOpcionalesPresentes: false);
    }

    private async Task VerificarInteraccion(
        string descripcion,
        string estado,
        PaquetesListosParaEntrega contrato,
        bool esperaOpcionalesPresentes)
    {
        var config = new PactConfig
        {
            PactDir = "../../../pacts/",
            DefaultJsonSettings = ContratoJsonOptions,
            Outputters = [new XunitOutput(_output)]
        };

        var pact = Pact.V4("ms-logistica-entrega", "ms-produccion-alimentos", config);
        var messagePact = pact.WithMessageInteractions();

        await messagePact
            .ExpectsToReceive(descripcion)
            .Given(estado)
            .WithJsonContent(contrato, ContratoJsonOptions)
            .VerifyAsync<PaquetesListosParaEntrega>(async mensaje =>
                await ProcesarPorElHandlerReal(mensaje, esperaOpcionalesPresentes));
    }

    // CT-04: el mensaje se pasa por el command/handler real (ProcesarPaquetesListosCommand /
    // ProcesarPaquetesListosCommandHandler), sustituyendo solo el puerto de persistencia por un
    // fake que captura -- lo que prueba este nivel es el contrato de entrada, no el efecto de
    // persistencia (eso ya lo cubre integracion, IT-04).
    private static async Task ProcesarPorElHandlerReal(PaquetesListosParaEntrega mensaje, bool esperaOpcionalesPresentes)
    {
        var store = new FakePaqueteRecibidoStore();
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IIntegrationEventPublisher).Assembly));
        services.AddSingleton<IPaqueteRecibidoStore>(store);
        using var provider = services.BuildServiceProvider();

        var sender = provider.GetRequiredService<ISender>();
        await sender.Send(new ProcesarPaquetesListosCommand(mensaje));

        Assert.Equal(mensaje.Paquetes.Count, store.Capturados.Count);

        var capturado = store.Capturados.Single();
        var paqueteOriginal = mensaje.Paquetes.Single();
        Assert.Equal(paqueteOriginal.PaqueteId, capturado.PaqueteId);
        Assert.Equal(paqueteOriginal.PacienteId, capturado.PacienteId);
        Assert.Equal(EstadoAsignacion.POR_ASIGNAR, capturado.Estado);

        if (esperaOpcionalesPresentes)
        {
            Assert.NotNull(capturado.DireccionEntrega.Referencia);
            Assert.NotNull(capturado.Etiqueta.CodigoQR);
        }
        else
        {
            Assert.Null(capturado.DireccionEntrega.Referencia);
            Assert.Null(capturado.Etiqueta.CodigoQR);
        }
    }

    private static PaquetesListosParaEntrega ConstruirContrato(bool conOpcionales)
    {
        var coordenadas = new CoordenadasDto(14.6349m, -90.5069m);
        var direccion = new DireccionGeoDto(
            "5a Avenida 10-20",
            "Zona 10",
            "Ciudad de Guatemala",
            conOpcionales ? "Frente al parque" : null,
            coordenadas);

        var fecha = new DateOnly(2026, 3, 12);
        var paqueteId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        var etiqueta = new EtiquetaDto(
            paqueteId,
            "Paciente De Prueba",
            "1234567890101",
            direccion,
            fecha,
            conOpcionales ? "QR-0001" : null);

        var paquete = new PaqueteListoDto(
            paqueteId,
            Guid.Parse("77777777-7777-7777-7777-777777777777"),
            "Paciente De Prueba",
            direccion,
            Guid.Parse("88888888-8888-8888-8888-888888888888"),
            etiqueta);

        return new PaquetesListosParaEntrega(fecha, [paquete]);
    }
}
