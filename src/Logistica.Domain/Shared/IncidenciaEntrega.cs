using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Shared;

public sealed record IncidenciaEntrega
{
    public DateTime FechaHora { get; private set; }
    public MotivoIncidencia Motivo { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public string? UrlFoto { get; private set; }

    private IncidenciaEntrega() { } // EF Core

    private IncidenciaEntrega(DateTime fechaHora, MotivoIncidencia motivo, string descripcion, string? urlFoto)
    {
        FechaHora = fechaHora;
        Motivo = motivo;
        Descripcion = descripcion;
        UrlFoto = urlFoto;
    }

    public static IncidenciaEntrega Crear(DateTime fechaHora, MotivoIncidencia motivo, string descripcion, string? urlFoto)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainException(IncidenciaEntregaErrors.DescripcionRequerida());

        return new IncidenciaEntrega(fechaHora, motivo, descripcion, urlFoto);
    }

    // Regla de owned types (§12.3 del maestro): una instancia no se comparte entre dos dueños
    public IncidenciaEntrega Clonar() => new(FechaHora, Motivo, Descripcion, UrlFoto);
}
