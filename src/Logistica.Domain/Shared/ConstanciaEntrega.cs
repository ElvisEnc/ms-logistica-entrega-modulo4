using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

public sealed record ConstanciaEntrega
{
    public DateTime FechaHora { get; private set; }
    public TipoConstancia Tipo { get; private set; }
    public string UrlEvidencia { get; private set; } = string.Empty;
    public string ReceptorNombre { get; private set; } = string.Empty;
    public Coordenadas? CoordenadasConfirmacion { get; private set; }

    private ConstanciaEntrega() { } // EF Core

    private ConstanciaEntrega(
        DateTime fechaHora,
        TipoConstancia tipo,
        string urlEvidencia,
        string receptorNombre,
        Coordenadas? coordenadasConfirmacion)
    {
        FechaHora = fechaHora;
        Tipo = tipo;
        UrlEvidencia = urlEvidencia;
        ReceptorNombre = receptorNombre;
        CoordenadasConfirmacion = coordenadasConfirmacion;
    }

    public static ConstanciaEntrega Crear(
        DateTime fechaHora,
        TipoConstancia tipo,
        string urlEvidencia,
        string receptorNombre,
        Coordenadas? coordenadasConfirmacion)
    {
        if (string.IsNullOrWhiteSpace(urlEvidencia))
            throw new DomainException(ConstanciaEntregaErrors.UrlRequerida()); // RN-16

        if (string.IsNullOrWhiteSpace(receptorNombre))
            throw new DomainException(ConstanciaEntregaErrors.ReceptorRequerido()); // RN-16

        return new ConstanciaEntrega(fechaHora, tipo, urlEvidencia, receptorNombre, coordenadasConfirmacion);
    }

    // Regla de owned types (§12.3 del maestro): una instancia no se comparte entre dos dueños
    public ConstanciaEntrega Clonar() =>
        new(FechaHora, Tipo, UrlEvidencia, ReceptorNombre, CoordenadasConfirmacion?.Clonar());
}
