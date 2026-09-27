using System.Net;
using Logistica.ContractTests.Testing;
using PactNet;
using PactNet.Matchers;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace Logistica.ContractTests.Proveedor;

// Par #11 (SISTEMA.md §13.5): ms-logistica-entrega es PROVEEDOR de ms-catering, con UNA
// interaccion (IncidenciaEntregaRegistrada). Mismo transporte y mismo shim que el par #10
// (INC-2, opcion B; CT-02); mismo tratamiento de pacto "bootstrap" (CT-08) porque ms-catering
// tampoco tiene todavia su propia infraestructura de contract tests.
[Trait("Capa", "Contrato")]
public sealed class CateringContractTests
{
    private const string RutaPacto = "../../../pacts/ms-catering-ms-logistica-entrega.json";

    private readonly ITestOutputHelper _output;

    public CateringContractTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task ParCatering_AutoraElPactoBootstrapYElProveedorRealLoVerifica()
    {
        await AutorarPactoBootstrap();
        await VerificarComoProveedorReal();
    }

    private async Task AutorarPactoBootstrap()
    {
        var config = new PactConfig
        {
            PactDir = "../../../pacts/",
            Outputters = [new XunitOutput(_output)]
        };

        var pact = Pact.V4("ms-catering", "ms-logistica-entrega", config);
        var pactBuilder = pact.WithHttpInteractions();

        pactBuilder
            .UponReceiving("una incidencia de entrega")
            .Given("una entrega tuvo una incidencia sin foto")
            .WithRequest(HttpMethod.Post, "/pact-provider/incidencia")
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new
            {
                rutaId = Match.Type(EscenariosProveedor.RutaGuid),
                paradaId = Match.Type(EscenariosProveedor.ParadaGuid),
                paqueteId = Match.Type(EscenariosProveedor.PaqueteGuid),
                pacienteId = Match.Type(EscenariosProveedor.PacienteGuid),
                pacienteNombre = Match.Type(EscenariosProveedor.PacienteNombre),
                contratoCateringId = Match.Type(EscenariosProveedor.ContratoCateringGuid),
                motivo = Match.Regex(
                    "DIRECCION_NO_ENCONTRADA",
                    "^(PACIENTE_AUSENTE|DIRECCION_NO_ENCONTRADA|PAQUETE_DANADO|RECHAZADO_POR_PACIENTE|OTRO)$"),
                descripcion = Match.Type("No se encontro la direccion registrada"),
                fechaHora = Match.Type("2026-03-10T13:30:00Z"),
                urlFoto = Match.Null() // CT-06: caso en null; el caso con valor lo cubre §15.3
            });

        await pactBuilder.VerifyAsync(async ctx =>
        {
            using var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            var respuesta = await client.PostAsync("/pact-provider/incidencia", null);
            respuesta.EnsureSuccessStatusCode();
        });
    }

    private async Task VerificarComoProveedorReal()
    {
        await using var host = new PactProviderHost();
        await host.StartAsync();

        var config = new PactVerifierConfig
        {
            Outputters = [new XunitOutput(_output)]
        };

        using var verifier = new PactVerifier("ms-logistica-entrega", config);
        verifier
            .WithHttpEndpoint(host.ServerUri)
            .WithFileSource(new FileInfo(RutaPacto))
            .Verify();
    }
}
