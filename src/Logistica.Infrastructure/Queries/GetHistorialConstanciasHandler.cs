using Joseco.DDD.Core.Results;
using Logistica.Application.Rutas.Queries;
using Logistica.Domain.Rutas;
using Logistica.Domain.Shared;
using Logistica.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistica.Infrastructure.Queries;

internal sealed class GetHistorialConstanciasHandler(LogisticaDbContext context)
    : IRequestHandler<GetHistorialConstanciasQuery, HistorialConstanciaDto[]>
{
    public async Task<HistorialConstanciaDto[]> Handle(GetHistorialConstanciasQuery request, CancellationToken cancellationToken)
    {
        if (request.Desde > request.Hasta)
            throw new DomainException(RutaErrors.RangoFechasInvalido());

        var rutas = await context.Rutas
            .Include("_paradas")
            .AsNoTracking()
            .Where(r => r.Fecha >= request.Desde && r.Fecha <= request.Hasta)
            .ToListAsync(cancellationToken);

        // Filtro prioritario por PacienteId (D-04, §10.2 DESIGN.md); PacienteNombre es una
        // comodidad de la mesa de reclamos, coincidencia parcial sin distinguir mayusculas, y NO
        // es la correlacion oficial (§9.3). Ambos filtros se aplican en memoria, DESPUES de
        // materializar (§10.2): el id tipado ya esta resuelto y no hay Select() traducido a SQL.
        var pacienteId = request.PacienteId is Guid guid ? PacienteId.From(guid) : null;

        return rutas
            .SelectMany(ruta => ruta.Paradas
                .Where(p => p.EstaResuelta())
                .Where(p => pacienteId is null || p.PacienteId == pacienteId)
                .Where(p => string.IsNullOrWhiteSpace(request.PacienteNombre)
                    || p.PacienteNombre.Contains(request.PacienteNombre, StringComparison.OrdinalIgnoreCase))
                .Select(parada => QueryDtoMapper.ToHistorialDto(ruta, parada)))
            .ToArray();
    }
}
