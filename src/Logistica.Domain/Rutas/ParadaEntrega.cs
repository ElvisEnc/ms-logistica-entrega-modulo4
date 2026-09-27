using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Rutas;

public sealed class ParadaEntrega : Entity
{
    public ParadaId ParadaId => ParadaId.From(Id);
    public PaqueteId PaqueteId { get; private set; } = null!;
    public PacienteId PacienteId { get; private set; } = null!;
    public string PacienteNombre { get; private set; } = string.Empty;
    public DireccionGeo DireccionEntrega { get; private set; } = null!;
    public ContratoId ContratoCateringId { get; private set; } = null!;
    public int Orden { get; private set; }
    public EstadoEntrega Estado { get; private set; }
    public ConstanciaEntrega? Constancia { get; private set; }
    public IncidenciaEntrega? Incidencia { get; private set; }

    private ParadaEntrega() { } // EF Core

    // Ningun Guid externo llega al constructor de un agregado (§5.3, JOSECO §2.3): se genera aqui.
    internal ParadaEntrega(PaqueteParaRuta paquete) : base(Guid.NewGuid())
    {
        PaqueteId = paquete.PaqueteId;
        PacienteId = paquete.PacienteId;
        PacienteNombre = paquete.PacienteNombre;
        DireccionEntrega = paquete.DireccionEntrega.Clonar(); // segunda clonacion (§5.4)
        ContratoCateringId = paquete.ContratoCateringId;
        Orden = 0;
        Estado = EstadoEntrega.PENDIENTE;
    }

    // internal (§5.2 DESIGN.md): toda operacion sobre una parada entra por la ruta, nunca directamente
    internal void AsignarOrden(int orden)
    {
        if (orden < 1)
            throw new DomainException(ParadaEntregaErrors.OrdenInvalido()); // I8 (RN-13, RN-15)

        Orden = orden;
    }

    internal void MarcarEnCamino()
    {
        Estado = EstadoEntrega.EN_CAMINO;
    }

    internal void ConfirmarEntrega(ConstanciaEntrega constancia)
    {
        if (EstaResuelta())
            throw new DomainException(ParadaEntregaErrors.TransicionInvalida()); // I4 (RN-16)

        if (constancia is null)
            throw new DomainException(ParadaEntregaErrors.ConstanciaRequerida()); // I5 (RN-16)

        Estado = EstadoEntrega.ENTREGADO;
        Constancia = constancia;
        Incidencia = null; // I5 (RN-16): la mitad que hace de I5 una garantia, no una convencion
    }

    internal void ReportarIncidencia(IncidenciaEntrega incidencia)
    {
        if (EstaResuelta())
            throw new DomainException(ParadaEntregaErrors.TransicionInvalida()); // I4 (RN-16)

        if (incidencia is null)
            throw new DomainException(ParadaEntregaErrors.IncidenciaRequerida()); // I5 (RN-16)

        Estado = EstadoEntrega.NO_ENTREGADO;
        Incidencia = incidencia;
        Constancia = null; // I5 (RN-16): simetrico de ConfirmarEntrega
    }

    public bool EstaResuelta() => Estado is EstadoEntrega.ENTREGADO or EstadoEntrega.NO_ENTREGADO;
}
