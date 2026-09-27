using System.Net;
using System.Net.Http.Json;
using Logistica.IntegrationTests.Setup;

namespace Logistica.IntegrationTests;

[Trait("Capa", "Integracion")]
public sealed class EntornoSmokeTests : IClassFixture<LogisticaWebApplicationFactory>
{
    private readonly LogisticaWebApplicationFactory _factory;

    public EntornoSmokeTests(LogisticaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetEntregas_SinRutasEseDia_DevuelveListaVaciaConHttp200()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/entregas?fecha=2026-01-01");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var estados = await response.Content.ReadFromJsonAsync<object[]>();
        Assert.NotNull(estados);
        Assert.Empty(estados);
    }
}
