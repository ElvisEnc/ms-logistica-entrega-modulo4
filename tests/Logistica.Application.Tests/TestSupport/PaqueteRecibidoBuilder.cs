using Logistica.Application.PaquetesRecibidos;

namespace Logistica.Application.Tests.TestSupport;

// Builder de datos de prueba (UT-12): construye un PaqueteRecibido POR_ASIGNAR valido para
// los escenarios de CrearRutaCommandHandlerTests (y, cuando corresponda,
// ProcesarPaquetesListosCommandHandlerTests).
public static class PaqueteRecibidoBuilder
{
    public static readonly DateOnly FechaPorDefecto = new(2026, 9, 27);

    public static PaqueteRecibido PorAsignar(Guid? paqueteId = null, DateOnly? fechaEntrega = null)
    {
        var id = paqueteId ?? Guid.NewGuid();
        var fecha = fechaEntrega ?? FechaPorDefecto;

        return new PaqueteRecibido
        {
            PaqueteId = id,
            PacienteId = Guid.NewGuid(),
            PacienteNombre = "Juan Pérez",
            DireccionEntrega = DireccionGeoBuilder.Valida(),
            ContratoCateringId = Guid.NewGuid(),
            Etiqueta = new EtiquetaPaquete
            {
                PaqueteId = id,
                NombrePaciente = "Juan Pérez",
                NroIdentificacion = "1234567",
                DireccionEntrega = DireccionGeoBuilder.Valida(),
                Fecha = fecha,
            },
            FechaEntrega = fecha,
            Estado = EstadoAsignacion.POR_ASIGNAR,
        };
    }
}
