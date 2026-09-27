using Logistica.Domain.Shared;

namespace Logistica.Application.PaquetesRecibidos;

// §9.4 DESIGN.md: read model, NO un agregado. No hereda de Entity/AggregateRoot, no tiene
// invariantes de dominio propias, no emite eventos y no se accede por IRepository<T>. Su
// correccion depende enteramente de la idempotencia de PaquetesListosParaEntrega (I12).
public sealed record PaqueteRecibido
{
    public Guid PaqueteId { get; init; } // clave primaria y clave natural de idempotencia (RN-17)
    public Guid PacienteId { get; init; } // Guid plano: el read model no usa IDs tipados
    public string PacienteNombre { get; init; } = string.Empty;
    public DireccionGeo DireccionEntrega { get; init; } = null!;
    public Guid ContratoCateringId { get; init; }
    public EtiquetaPaquete Etiqueta { get; init; } = null!;
    public DateOnly FechaEntrega { get; init; } // tomada de la raiz del evento (I10)
    public EstadoAsignacion Estado { get; init; }
}
