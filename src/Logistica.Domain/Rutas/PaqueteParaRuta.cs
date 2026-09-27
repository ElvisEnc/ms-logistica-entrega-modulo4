using Joseco.DDD.Core.Results;
using Logistica.Domain.Shared;

namespace Logistica.Domain.Rutas;

// VO de entrada al agregado (§5.4): no se persiste, vive solo durante RutaEntrega.Crear.
public sealed record PaqueteParaRuta
{
    public PaqueteId PaqueteId { get; }
    public PacienteId PacienteId { get; }
    public string PacienteNombre { get; }
    public DireccionGeo DireccionEntrega { get; }
    public ContratoId ContratoCateringId { get; }

    private PaqueteParaRuta(
        PaqueteId paqueteId,
        PacienteId pacienteId,
        string pacienteNombre,
        DireccionGeo direccionEntrega,
        ContratoId contratoCateringId)
    {
        PaqueteId = paqueteId;
        PacienteId = pacienteId;
        PacienteNombre = pacienteNombre;
        DireccionEntrega = direccionEntrega;
        ContratoCateringId = contratoCateringId;
    }

    public static PaqueteParaRuta Crear(
        PaqueteId paqueteId,
        PacienteId pacienteId,
        string pacienteNombre,
        DireccionGeo direccionEntrega,
        ContratoId contratoCateringId)
    {
        if (paqueteId is null)
            throw new DomainException(PaqueteParaRutaErrors.PaqueteIdRequerido());

        if (pacienteId is null)
            throw new DomainException(PaqueteParaRutaErrors.PacienteIdRequerido()); // I14 (RN-16, RN-23)

        if (string.IsNullOrWhiteSpace(pacienteNombre))
            throw new DomainException(PaqueteParaRutaErrors.PacienteNombreRequerido());

        if (direccionEntrega is null)
            throw new DomainException(PaqueteParaRutaErrors.DireccionRequerida());

        if (contratoCateringId is null)
            throw new DomainException(PaqueteParaRutaErrors.ContratoCateringRequerido()); // I14 (RN-16, RN-23)

        return new PaqueteParaRuta(paqueteId, pacienteId, pacienteNombre, direccionEntrega, contratoCateringId);
    }
}
