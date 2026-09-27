using Logistica.Domain.Shared;

namespace Logistica.Application.PaquetesRecibidos;

// §9.4 DESIGN.md: replica completa de la Etiqueta de SISTEMA.md §9.11. Dato de BC5, no del
// dominio de BC6: no tiene validaciones propias (§5.4).
public sealed record EtiquetaPaquete
{
    public Guid PaqueteId { get; init; }
    public string NombrePaciente { get; init; } = string.Empty;
    public string NroIdentificacion { get; init; } = string.Empty;
    public DireccionGeo DireccionEntrega { get; init; } = null!;
    public DateOnly Fecha { get; init; }
    public string? CodigoQR { get; init; }
}
