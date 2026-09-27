using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.TestSupport;

// Builder de datos de prueba (UT-12): construye una DireccionGeo valida para escenarios que no
// ejercitan las invariantes propias de DireccionGeo. Espejo del builder homonimo de
// Logistica.Domain.Tests (cada proyecto de test tiene su propio TestSupport/, §6 del MAPA).
public static class DireccionGeoBuilder
{
    public static DireccionGeo Valida() =>
        DireccionGeo.Crear("Av. San Martín", "Equipetrol", "Santa Cruz de la Sierra", null,
            Coordenadas.Crear(-17.783206m, -63.182140m));
}
