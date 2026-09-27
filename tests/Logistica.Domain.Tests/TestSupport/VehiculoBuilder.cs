using Logistica.Domain.Shared;

namespace Logistica.Domain.Tests.TestSupport;

// Builder de datos de prueba (UT-12): construye un Vehiculo válido para
// escenarios que no ejercitan las invariantes propias de Vehiculo.
public static class VehiculoBuilder
{
    public static Vehiculo Valido(int capacidadPaquetes = 10) =>
        Vehiculo.Crear("Motocicleta", "1234-ABC", capacidadPaquetes);
}
