using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;
using Logistica.Application.Abstractions;
using Logistica.Domain.Repartidores;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;
using MediatR;

namespace Logistica.Application.Rutas.CrearRuta;

// §6.2 DESIGN.md: aqui viven las cuatro invariantes de orquestacion que RutaEntrega no puede
// garantizar por si sola porque cruzan agregados o dependen de un repositorio: I2 (mitad de
// capacidad), I9, I10 e I11. El orden de comprobacion es deliberado (§12.1): el error devuelto
// es siempre la causa raiz, nunca una consecuencia.
internal sealed class CrearRutaCommandHandler(
    IRepartidorRepository repartidores,
    IRutaEntregaRepository rutas,
    IPaqueteRecibidoStore paquetesRecibidos,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CrearRutaCommand, Guid>
{
    public async Task<Guid> Handle(CrearRutaCommand request, CancellationToken cancellationToken)
    {
        var repartidorId = RepartidorId.From(request.RepartidorId);

        var repartidor = await repartidores.GetByIdAsync(repartidorId.Value)
            ?? throw new DomainException(RepartidorErrors.NoEncontrado()); // busqueda de raiz -> 404

        if (await repartidores.TieneRutaActivaAsync(repartidorId))
            throw new DomainException(RutaErrors.RepartidorConRutaActiva()); // I11 (RN-27)

        // I9/I10 en una sola consulta, SIN filtrar por fecha (§6.3): un paqueteId ausente del
        // pool es I9 (ya asignado o inexistente); uno presente con otra fecha es I10.
        var pool = await paquetesRecibidos.ObtenerPorAsignarAsync(cancellationToken);
        var poolPorId = pool.ToDictionary(p => p.PaqueteId);

        var paquetesParaRuta = new List<PaqueteParaRuta>(request.PaqueteIds.Count);
        foreach (var paqueteId in request.PaqueteIds)
        {
            if (!poolPorId.TryGetValue(paqueteId, out var paquete))
                throw new DomainException(RutaErrors.PaqueteYaAsignado()); // I9 (RN-17)

            if (paquete.FechaEntrega != request.Fecha)
                throw new DomainException(RutaErrors.PaquetesFechaDistinta()); // I10 (RN-10)

            paquetesParaRuta.Add(PaqueteParaRuta.Crear(
                PaqueteId.From(paquete.PaqueteId),
                PacienteId.From(paquete.PacienteId),
                paquete.PacienteNombre,
                paquete.DireccionEntrega.Clonar(), // §5.4: se clona antes de entrar al agregado
                ContratoId.From(paquete.ContratoCateringId)));
        }

        if (!repartidor.TieneCapacidadPara(paquetesParaRuta.Count))
            throw new DomainException(RutaErrors.CapacidadInsuficiente()); // I2 (RN-27, RN-13) (mitad de capacidad)

        var ruta = RutaEntrega.Crear(request.Fecha, repartidorId, paquetesParaRuta); // I1 en Domain
        repartidor.AsignarRuta(); // I2 (RN-27) (mitad de disponibilidad), refuerzo de I11 (RN-27)

        await rutas.AddAsync(ruta);
        await paquetesRecibidos.MarcarAsignadosAsync(request.PaqueteIds, cancellationToken); // I9 (RN-17)

        await unitOfWork.CommitAsync(cancellationToken);

        return ruta.RutaId.Value;
    }
}
