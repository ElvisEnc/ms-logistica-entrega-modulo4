using Logistica.Application.IntegrationEvents;

namespace Logistica.Application.Tests.TestSupport;

// Builder de datos de prueba (UT-12): construye el contrato de entrada del endpoint de
// integracion entrante (§11.8 DESIGN.md, SISTEMA.md §9.11) — PaquetesListosParaEntrega y su
// PaqueteListoDto — con valores validos por defecto y overrides para las ramas de I14.
public static class PaqueteListoDtoBuilder
{
    public static readonly DateOnly FechaPorDefecto = new(2026, 9, 27);

    private static readonly CoordenadasDto CoordenadasValidas = new(-17.783206m, -63.182140m);

    private static readonly DireccionGeoDto DireccionValida =
        new("Av. San Martín", "Equipetrol", "Santa Cruz de la Sierra", null, CoordenadasValidas);

    public static PaqueteListoDto Valido(
        Guid? paqueteId = null, Guid? pacienteId = null, Guid? contratoCateringId = null)
    {
        var id = paqueteId ?? Guid.NewGuid();

        return new PaqueteListoDto(
            id,
            pacienteId ?? Guid.NewGuid(),
            "Juan Pérez",
            DireccionValida,
            contratoCateringId ?? Guid.NewGuid(),
            new EtiquetaDto(id, "Juan Pérez", "1234567", DireccionValida, FechaPorDefecto, null));
    }

    public static PaquetesListosParaEntrega Lote(params PaqueteListoDto[] paquetes) =>
        new(FechaPorDefecto, paquetes);
}
