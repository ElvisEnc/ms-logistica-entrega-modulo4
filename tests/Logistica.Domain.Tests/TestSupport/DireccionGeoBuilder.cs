using Logistica.Domain.Shared;

namespace Logistica.Domain.Tests.TestSupport;

// Builder de datos de prueba (UT-12): construye una DireccionGeo válida para
// escenarios que no ejercitan las invariantes propias de DireccionGeo.
public static class DireccionGeoBuilder
{
    public static DireccionGeo Valida() =>
        DireccionGeo.Crear("Av. San Martín", "Equipetrol", "Santa Cruz de la Sierra", null,
            Coordenadas.Crear(-17.783206m, -63.182140m));
}
