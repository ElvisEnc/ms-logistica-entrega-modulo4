using System.Net;
using Logistica.ContractTests.Testing;
using PactNet;
using PactNet.Matchers;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace Logistica.ContractTests.Proveedor;

// Par #10 (SISTEMA.md §13.5): ms-logistica-entrega es PROVEEDOR de ms-pacientes, con DOS
// interacciones (EntregaConfirmada, IncidenciaEntregaRegistrada) -- es el par minimo viable de
// la actividad ("al menos dos solicitudes... verificadas desde el provider").
//
// Transporte: Pact HTTP sobre el shim de solo test PactProviderHost (INC-2, opcion B; CT-02).
// ms-pacientes todavia no tiene su propia infraestructura de contract tests, asi que el lado
// consumidor de este pacto se autora AQUI, en nombre de ms-pacientes (CT-08, "bootstrap"):
// cuando ms-pacientes implemente su propia Tarea 3, su pacto real sustituye a este sin que
// ms-logistica-entrega necesite cambiar su lado (la verificacion de proveedor, mas abajo).
[Trait("Capa", "Contrato")]
public sealed class PacientesContractTests
{
    private const string RutaPacto = "../../../pacts/ms-pacientes-ms-logistica-entrega.json";

    private readonly ITestOutputHelper _output;

    public PacientesContractTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task ParPacientes_AutoraElPactoBootstrapYElProveedorRealLoVerifica()
    {
        await AutorarPactoBootstrap();
        await VerificarComoProveedorReal();
    }

    // Lado consumidor -- BOOTSTRAP en nombre de ms-pacientes (CT-08). Usa el mock server propio
    // de PactNet, no el shim real: su unico proposito es declarar la forma esperada y producir
    // pacts/ms-pacientes-ms-logistica-entrega.json.
    private async Task AutorarPactoBootstrap()
    {
        var config = new PactConfig
        {
            PactDir = "../../../pacts/",
            Outputters = [new XunitOutput(_output)]
        };

        var pact = Pact.V4("ms-pacientes", "ms-logistica-entrega", config);
        var pactBuilder = pact.WithHttpInteractions();

        pactBuilder
            .UponReceiving("una confirmacion de entrega")
            .Given("una entrega fue confirmada con evidencia y coordenadas")
            .WithRequest(HttpMethod.Post, "/pact-provider/entrega-confirmada")
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
                constancia = new
                {
                    tipo = Match.Regex("FOTO", "^(FOTO|FIRMA)$"),
                    urlEvidencia = Match.Type("https://evidencias.local/foto-1.jpg"),
                    receptorNombre = Match.Type("Receptor De Prueba"),
                    fechaHora = Match.Type("2026-03-10T12:00:00Z"),
                    coordenadasConfirmacion = new
                    {
                        latitud = Match.Decimal(14.6349m),
                        longitud = Match.Decimal(-90.5069m)
                    }
                }
            });

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

            var entrega = await client.PostAsync("/pact-provider/entrega-confirmada", null);
            entrega.EnsureSuccessStatusCode();

            var incidencia = await client.PostAsync("/pact-provider/incidencia", null);
            incidencia.EnsureSuccessStatusCode();
        });
    }

    // Lado proveedor -- REAL: el shim ejecuta el handler y el traductor reales de este
    // repositorio (CT-03/CT-05); esta es la verificacion que la actividad exige.
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
