using Joseco.DDD.Core.Results;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Tests.Shared;

[Trait("Capa", "Unit")]
public class ConstanciaEntregaTests
{
    [Fact]
    public void Crear_con_url_evidencia_vacia_lanza_CONSTANCIA_URL_REQUERIDA()
    {
        // Arrange
        var fechaHora = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);
        var tipo = TipoConstancia.FOTO;
        var urlEvidencia = string.Empty;
        var receptorNombre = "Juan Perez";
        Coordenadas? coordenadasConfirmacion = null;

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            ConstanciaEntrega.Crear(fechaHora, tipo, urlEvidencia, receptorNombre, coordenadasConfirmacion)); // RN-16

        // Assert
        Assert.Equal("CONSTANCIA_URL_REQUERIDA", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Fact]
    public void Crear_con_receptor_nombre_vacio_lanza_CONSTANCIA_RECEPTOR_REQUERIDO()
    {
        // Arrange
        var fechaHora = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);
        var tipo = TipoConstancia.FIRMA;
        var urlEvidencia = "https://storage.local/evidencias/foto1.jpg";
        var receptorNombre = string.Empty;
        Coordenadas? coordenadasConfirmacion = null;

        // Act
        var excepcion = Assert.Throws<DomainException>(() =>
            ConstanciaEntrega.Crear(fechaHora, tipo, urlEvidencia, receptorNombre, coordenadasConfirmacion)); // RN-16

        // Assert
        Assert.Equal("CONSTANCIA_RECEPTOR_REQUERIDO", excepcion.Error.Code);
        Assert.Equal(ErrorType.Validation, excepcion.Error.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("con-coordenadas")]
    public void CoordenadasConfirmacion_es_opcional(string? escenario)
    {
        // Arrange
        var fechaHora = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);
        var tipo = TipoConstancia.FOTO;
        var urlEvidencia = "https://storage.local/evidencias/foto1.jpg";
        var receptorNombre = "Juan Perez";
        var coordenadasConfirmacion = escenario is null
            ? null
            : Coordenadas.Crear(-17.783206m, -63.182140m);

        // Act
        var constancia = ConstanciaEntrega.Crear(fechaHora, tipo, urlEvidencia, receptorNombre, coordenadasConfirmacion); // RN-16

        // Assert
        Assert.Equal(coordenadasConfirmacion, constancia.CoordenadasConfirmacion);
    }

    [Fact]
    public void Crear_con_todos_los_campos_validos_construye_la_constancia()
    {
        // Arrange
        var fechaHora = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);
        var tipo = TipoConstancia.FOTO;
        var urlEvidencia = "https://storage.local/evidencias/foto1.jpg";
        var receptorNombre = "Juan Perez";
        var coordenadasConfirmacion = Coordenadas.Crear(-17.783206m, -63.182140m);

        // Act
        var constancia = ConstanciaEntrega.Crear(fechaHora, tipo, urlEvidencia, receptorNombre, coordenadasConfirmacion); // RN-16

        // Assert
        Assert.Equal(fechaHora, constancia.FechaHora);
        Assert.Equal(tipo, constancia.Tipo);
        Assert.Equal(urlEvidencia, constancia.UrlEvidencia);
        Assert.Equal(receptorNombre, constancia.ReceptorNombre);
        Assert.Equal(coordenadasConfirmacion, constancia.CoordenadasConfirmacion);
    }
}
