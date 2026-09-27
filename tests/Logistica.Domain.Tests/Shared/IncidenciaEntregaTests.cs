using Joseco.DDD.Core.Results;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Tests.Shared;

[Trait("Capa", "Unit")]
public class IncidenciaEntregaTests
{
    [Fact]
    public void Crear_con_descripcion_vacia_lanza_INCIDENCIA_DESCRIPCION_REQUERIDA()
    {
        // Arrange
        var fechaHora = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);
        var motivo = MotivoIncidencia.PACIENTE_AUSENTE;
        var descripcion = string.Empty;
        string? urlFoto = null;

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            IncidenciaEntrega.Crear(fechaHora, motivo, descripcion, urlFoto));

        // Assert
        Assert.Equal("INCIDENCIA_DESCRIPCION_REQUERIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("con-url-foto")]
    public void UrlFoto_es_opcional(string? escenario)
    {
        // Arrange
        var fechaHora = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);
        var motivo = MotivoIncidencia.PAQUETE_DANADO;
        var descripcion = "El paquete llego danado";
        var urlFoto = escenario is null
            ? null
            : "https://storage.local/evidencias/incidencia1.jpg";

        // Act
        var incidencia = IncidenciaEntrega.Crear(fechaHora, motivo, descripcion, urlFoto);

        // Assert
        Assert.Equal(urlFoto, incidencia.UrlFoto);
    }

    [Fact]
    public void Crear_con_todos_los_campos_validos_construye_la_incidencia()
    {
        // Arrange
        var fechaHora = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);
        var motivo = MotivoIncidencia.DIRECCION_NO_ENCONTRADA;
        var descripcion = "No se encontro la direccion indicada";
        var urlFoto = "https://storage.local/evidencias/incidencia1.jpg";

        // Act
        var incidencia = IncidenciaEntrega.Crear(fechaHora, motivo, descripcion, urlFoto);

        // Assert
        Assert.Equal(fechaHora, incidencia.FechaHora);
        Assert.Equal(motivo, incidencia.Motivo);
        Assert.Equal(descripcion, incidencia.Descripcion);
        Assert.Equal(urlFoto, incidencia.UrlFoto);
    }
}
