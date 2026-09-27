using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Repartidores;

public static class RepartidorErrors
{
    public static Error NoDisponible() =>
        new("I2_REPARTIDOR_NO_DISPONIBLE", "El repartidor no esta disponible", ErrorType.Conflict); // I2

    // Busqueda de raiz por repositorio (§13.2 DESIGN.md) — lanzado desde CrearRutaCommandHandler
    // y desde las dos politicas de liberacion, Application
    public static Error NoEncontrado() =>
        new("REPARTIDOR_NO_ENCONTRADO", "No se encontro el repartidor solicitado", ErrorType.NotFound);

    public static Error NombreRequerido() =>
        new("REPARTIDOR_NOMBRE_REQUERIDO", "El nombre del repartidor es obligatorio", ErrorType.Validation);

    public static Error TelefonoRequerido() =>
        new("REPARTIDOR_TELEFONO_REQUERIDO", "El telefono del repartidor es obligatorio", ErrorType.Validation);

    public static Error VehiculoRequerido() =>
        new("REPARTIDOR_VEHICULO_REQUERIDO", "El vehiculo del repartidor es obligatorio", ErrorType.Validation);
}
