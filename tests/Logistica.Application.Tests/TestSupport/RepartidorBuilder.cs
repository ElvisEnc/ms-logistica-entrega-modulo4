using Logistica.Domain.Repartidores;
using Logistica.Domain.Shared;

namespace Logistica.Application.Tests.TestSupport;

// Builder de datos de prueba (UT-12): construye un Repartidor disponible con capacidad de
// vehiculo configurable, valido para los escenarios de CrearRutaCommandHandlerTests.
public static class RepartidorBuilder
{
    public static Repartidor Disponible(int capacidadPaquetes = 10) =>
        Repartidor.Registrar(
            "Juan Pérez",
            "70000000",
            Vehiculo.Crear("Motocicleta", "1234-ABC", capacidadPaquetes));
}
