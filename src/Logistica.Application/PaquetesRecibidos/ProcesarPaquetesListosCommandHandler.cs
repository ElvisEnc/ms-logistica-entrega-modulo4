using Joseco.DDD.Core.Results;
using Logistica.Application.Abstractions;
using Logistica.Application.IntegrationEvents;
using Logistica.Domain.Shared;
using MediatR;

namespace Logistica.Application.PaquetesRecibidos;

// §9.2 DESIGN.md: PaqueteRecibido no es un agregado, asi que este handler no toca ningun
// repositorio de Domain ni hace commit por UnitOfWork; persiste directamente en el store del
// read model. I12 (idempotencia) vive dentro de IPaqueteRecibidoStore.UpsertAsync, en Infrastructure.
internal sealed class ProcesarPaquetesListosCommandHandler(IPaqueteRecibidoStore store)
    : IRequestHandler<ProcesarPaquetesListosCommand>
{
    public async Task Handle(ProcesarPaquetesListosCommand request, CancellationToken cancellationToken)
    {
        foreach (var paquete in request.Contrato.Paquetes)
        {
            // Defensa en profundidad (§9.2, §12.5 del maestro): BC5 no deberia enviarlos vacios,
            // pero BC6 los comprueba igual, ANTES de materializar los VO de dominio.
            if (paquete.PacienteId == Guid.Empty)
                throw new DomainException(PaqueteRecibidoErrors.PacienteIdRequerido()); // I14

            if (paquete.ContratoCateringId == Guid.Empty)
                throw new DomainException(PaqueteRecibidoErrors.ContratoCateringRequerido()); // I14

            var etiqueta = new EtiquetaPaquete
            {
                PaqueteId = paquete.Etiqueta.PaqueteId,
                NombrePaciente = paquete.Etiqueta.NombrePaciente,
                NroIdentificacion = paquete.Etiqueta.NroIdentificacion,
                DireccionEntrega = MapearDireccion(paquete.Etiqueta.DireccionEntrega),
                Fecha = paquete.Etiqueta.Fecha,
                CodigoQR = paquete.Etiqueta.CodigoQR
            };

            var paqueteRecibido = new PaqueteRecibido
            {
                PaqueteId = paquete.PaqueteId,
                PacienteId = paquete.PacienteId,
                PacienteNombre = paquete.PacienteNombre,
                DireccionEntrega = MapearDireccion(paquete.DireccionEntrega),
                ContratoCateringId = paquete.ContratoCateringId,
                Etiqueta = etiqueta,
                FechaEntrega = request.Contrato.Fecha, // raiz del evento, no de cada paquete (I10)
                Estado = EstadoAsignacion.POR_ASIGNAR
            };

            await store.UpsertAsync(paqueteRecibido, cancellationToken); // I12
        }
    }

    // Las coordenadas y los campos obligatorios de la direccion se validan aqui, en el
    // constructor del VO de dominio (RN-15): COORDENADAS_*_INVALIDA, DIRECCION_*_REQUERIDA.
    private static DireccionGeo MapearDireccion(DireccionGeoDto dto) =>
        DireccionGeo.Crear(
            dto.Calle,
            dto.Zona,
            dto.Ciudad,
            dto.Referencia,
            Coordenadas.Crear(dto.Coordenadas.Latitud, dto.Coordenadas.Longitud));
}
