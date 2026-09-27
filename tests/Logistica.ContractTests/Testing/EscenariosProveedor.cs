using Logistica.Domain.Rutas.Events;
using Logistica.Domain.Shared;

namespace Logistica.ContractTests.Testing;

// CT-03: valores fijos y deterministas para construir los eventos de dominio reales que el
// shim de proveedor (PactProviderHost) publica. Nunca se construye a mano el JSON esperado:
// se construye el evento de dominio real y se deja que el traductor real
// (DomainToIntegrationMapper, invocado vía MediatR) lo convierta.
internal static class EscenariosProveedor
{
    public static readonly Guid RutaGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ParadaGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid PaqueteGuid = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid PacienteGuid = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid ContratoCateringGuid = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public const string PacienteNombre = "Paciente De Prueba";

    public static EntregaConfirmada EntregaConfirmada()
    {
        var constancia = ConstanciaEntrega.Crear(
            new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc),
            TipoConstancia.FOTO,
            "https://evidencias.local/foto-1.jpg",
            "Receptor De Prueba",
            Coordenadas.Crear(14.6349m, -90.5069m));

        return new EntregaConfirmada(
            RutaId.From(RutaGuid),
            ParadaId.From(ParadaGuid),
            PaqueteId.From(PaqueteGuid),
            PacienteId.From(PacienteGuid),
            PacienteNombre,
            constancia);
    }

    public static IncidenciaEntregaRegistrada IncidenciaEntregaRegistrada()
    {
        var incidencia = IncidenciaEntrega.Crear(
            new DateTime(2026, 3, 10, 13, 30, 0, DateTimeKind.Utc),
            MotivoIncidencia.DIRECCION_NO_ENCONTRADA,
            "No se encontro la direccion registrada",
            urlFoto: null); // caso opcional en null (CT-06; el caso con valor lo cubre §15.3)

        return new IncidenciaEntregaRegistrada(
            RutaId.From(RutaGuid),
            ParadaId.From(ParadaGuid),
            PaqueteId.From(PaqueteGuid),
            PacienteId.From(PacienteGuid),
            PacienteNombre,
            ContratoId.From(ContratoCateringGuid),
            incidencia);
    }
}
