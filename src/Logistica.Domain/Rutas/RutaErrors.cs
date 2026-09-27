using Joseco.DDD.Core.Results;

namespace Logistica.Domain.Rutas;

// §6.2, §13.3 del DESIGN.md: los errores que solo se lanzan desde Application (I9, I10, I11,
// la mitad de capacidad de I2, EVIDENCIA_ARCHIVO_REQUERIDO, RANGO_FECHAS_INVALIDO,
// RUTA_NO_ENCONTRADA) se declaran igualmente aqui para reutilizar el mismo Error desde ambas capas.
public static class RutaErrors
{
    public static Error SinParadas() =>
        new("I1_RUTA_SIN_PARADAS", "La ruta debe tener al menos una parada", ErrorType.Validation); // I1 (RN-13)

    public static Error CapacidadInsuficiente() =>
        new("I2_REPARTIDOR_NO_DISPONIBLE", "El vehiculo del repartidor no tiene capacidad para los paquetes asignados", ErrorType.Conflict); // I2 (RN-27, RN-13) — lanzado desde CrearRutaCommandHandler, Application

    public static Error NoPendiente() =>
        new("I3_RUTA_NO_PENDIENTE", "La ruta debe estar pendiente para realizar esta operacion", ErrorType.Conflict); // I3 (RN-13, RN-15)

    public static Error NoOptimizada() =>
        new("I3_RUTA_NO_OPTIMIZADA", "La ruta debe estar optimizada antes de iniciarse", ErrorType.Conflict); // I3 (RN-13, RN-15)

    public static Error TransicionInvalida() =>
        new("I4_TRANSICION_INVALIDA", "La ruta debe estar en camino para confirmar o reportar una entrega", ErrorType.Conflict); // I4 (RN-16)

    public static Error ParadasPendientes() =>
        new("I6_PARADAS_PENDIENTES", "La ruta no puede completarse porque tiene paradas sin resolver", ErrorType.Conflict); // I6 (RN-16)

    public static Error OrdenParadasInvalido() =>
        new("I8_ORDEN_PARADAS_INVALIDO", "El orden calculado para las paradas no forma una secuencia valida", ErrorType.Validation); // I8 (RN-13, RN-15)

    public static Error PaqueteYaAsignado() =>
        new("I9_PAQUETE_YA_ASIGNADO", "Uno o mas paquetes ya estan asignados a otra ruta", ErrorType.Conflict); // I9 (RN-17) — lanzado desde CrearRutaCommandHandler, Application

    public static Error PaquetesFechaDistinta() =>
        new("I10_PAQUETES_FECHA_DISTINTA", "Todos los paquetes de la ruta deben corresponder a la misma fecha de entrega", ErrorType.Validation); // I10 (RN-10) — lanzado desde CrearRutaCommandHandler, Application

    public static Error RepartidorConRutaActiva() =>
        new("I11_REPARTIDOR_CON_RUTA_ACTIVA", "El repartidor ya tiene una ruta activa", ErrorType.Conflict); // I11 (RN-27) — lanzado desde CrearRutaCommandHandler, Application

    public static Error NoCancelable() =>
        new("I13_RUTA_NO_CANCELABLE", "La ruta solo puede cancelarse desde pendiente o en camino", ErrorType.Conflict); // I13 (RN-13)

    public static Error MotivoRequerido() =>
        new("I13_MOTIVO_REQUERIDO", "El motivo de cancelacion es obligatorio", ErrorType.Validation); // I13 (RN-13)

    public static Error ParadaNoEncontrada() =>
        new("PARADA_NO_ENCONTRADA", "La parada indicada no pertenece a esta ruta", ErrorType.Validation); // precondicion del comando, nunca NotFound

    public static Error RutaNoEncontrada() =>
        new("RUTA_NO_ENCONTRADA", "No se encontro la ruta solicitada", ErrorType.NotFound);

    public static Error EvidenciaArchivoRequerido() =>
        new("EVIDENCIA_ARCHIVO_REQUERIDO", "El archivo de evidencia es obligatorio para confirmar la entrega", ErrorType.Validation); // RN-16

    public static Error EvidenciaExtensionNoPermitida() =>
        new("EVIDENCIA_EXTENSION_NO_PERMITIDA", "La extension del archivo de evidencia no esta permitida", ErrorType.Validation); // D-09 (§19.2) — INC-1, Opcion A: lanzado desde Application

    public static Error EvidenciaTamanoExcedido() =>
        new("EVIDENCIA_TAMANO_EXCEDIDO", "El archivo de evidencia excede el tamano maximo permitido", ErrorType.Validation); // D-09 (§19.2) — INC-1, Opcion A: lanzado desde Application

    public static Error RangoFechasInvalido() =>
        new("RANGO_FECHAS_INVALIDO", "La fecha 'desde' no puede ser posterior a la fecha 'hasta'", ErrorType.Validation);
}
