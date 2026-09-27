# MAPA de unit tests — ms-logistica-entrega (BC6)

Inventario específico que alimenta `/unit-tests`. Fuentes: `docs/DESIGN.md` §5, §6
(incluida §6.2), §7, §8.2, §13, §15.1–15.3, y el código real de
`src/Logistica.Domain` y `src/Logistica.Application` (verificado con
`mcp__codegraph__codegraph_explore`, no leyendo archivo por archivo).

**Notación de origen**, en la columna «Origen» de las tablas de la sección 4:

- **DESIGN** — el nombre del test aparece literal en `docs/DESIGN.md` §15.1–15.3.
- **DESIGN (código)** — `DESIGN.md` no da el nombre del método, pero sí el código de
  error o la invariante que lo respalda (§13/§6); el nombre final lo decide
  `test-writer` siguiendo UT-04.
- **Derivado** — el conteo total de la suite en `DESIGN.md` §15.1 («Cobertura
  adicional») es mayor que la suma de tests con nombre o código explícito; esta
  fila cubre el resto, derivada del código real verificado con CodeGraph, no de
  una cita textual de `DESIGN.md`.
- **Gap** — el código real tiene una rama o un handler que ninguna tabla de
  `docs/DESIGN.md` §15.1–15.3 pide probar. No es una incoherencia (no hay
  contradicción entre `SISTEMA.md`/`DESIGN.md`/código, y no bloquea ninguna
  decisión de dominio): es una fila que falta en el propio plan de pruebas y que
  este MAPA añade para que la cobertura real no dependa de que alguien se acuerde.

**Columna «Estado»**: vacía en cada fila de la sección 4. La rellena el pipeline
(`test-writer`/`test-reviewer`) con `Pendiente` (valor inicial, implícito),
`Hecho` o `INC-<n>` si el test reveló una incoherencia y quedó con
`[Fact(Skip = "INC-<n>")]`.

---

## 1. Objetivos del dominio

### Agregados

| Objetivo | Archivo | Notas |
|---|---|---|
| `RutaEntrega` (raíz) | `src/Logistica.Domain/Rutas/RutaEntrega.cs` | `Crear`, `Optimizar`, `Iniciar`, `ConfirmarEntrega`, `ReportarIncidencia`, `Completar`, `Cancelar`, `ObtenerSiguienteParada`, `BuscarParada` (privado) |
| `Repartidor` (raíz) | `src/Logistica.Domain/Repartidores/Repartidor.cs` | `Registrar`, `EstaDisponible`, `TieneCapacidadPara`, `AsignarRuta`, `Liberar` |

### Entidad hija

| Objetivo | Archivo | Notas |
|---|---|---|
| `ParadaEntrega` | `src/Logistica.Domain/Rutas/ParadaEntrega.cs` | Constructor `internal`, `AsignarOrden`, `MarcarEnCamino`, `ConfirmarEntrega`, `ReportarIncidencia`, `EstaResuelta` — todos `internal` salvo `EstaResuelta` (D-12 §19.3: `MarcarEnCamino` es público en el código real, desviación de severidad baja ya documentada, no se repite aquí) |

### Value Objects

| VO | Archivo | Factory | Errores (código, `ErrorType`) |
|---|---|---|---|
| `Coordenadas` | `src/Logistica.Domain/Shared/Coordenadas.cs` | `Crear(latitud, longitud)` | `COORDENADAS_LATITUD_INVALIDA`, `COORDENADAS_LONGITUD_INVALIDA` (`Validation`) |
| `DireccionGeo` | `src/Logistica.Domain/Shared/DireccionGeo.cs` | `Crear(calle, zona, ciudad, referencia?, coordenadas)` | `DIRECCION_CALLE_REQUERIDA`, `DIRECCION_ZONA_REQUERIDA`, `DIRECCION_CIUDAD_REQUERIDA`, `DIRECCION_COORDENADAS_REQUERIDAS` (`Validation`) |
| `Vehiculo` | `src/Logistica.Domain/Shared/Vehiculo.cs` | `Crear(tipo, placa, capacidadPaquetes)` | `VEHICULO_TIPO_REQUERIDO`, `VEHICULO_PLACA_REQUERIDA`, `VEHICULO_CAPACIDAD_INVALIDA` (`Validation`) |
| `ConstanciaEntrega` | `src/Logistica.Domain/Shared/ConstanciaEntrega.cs` | `Crear(fechaHora, tipo, urlEvidencia, receptorNombre, coordenadasConfirmacion?)` | `CONSTANCIA_URL_REQUERIDA`, `CONSTANCIA_RECEPTOR_REQUERIDO` (`Validation`) |
| `IncidenciaEntrega` | `src/Logistica.Domain/Shared/IncidenciaEntrega.cs` | `Crear(fechaHora, motivo, descripcion, urlFoto?)` | `INCIDENCIA_DESCRIPCION_REQUERIDA` (`Validation`) |
| `PaqueteParaRuta` | `src/Logistica.Domain/Rutas/PaqueteParaRuta.cs` | `Crear(paqueteId, pacienteId, pacienteNombre, direccionEntrega, contratoCateringId)` | `PAQUETE_RUTA_PAQUETE_ID_REQUERIDO`, `PAQUETE_RUTA_PACIENTE_ID_REQUERIDO` (I14), `PAQUETE_RUTA_PACIENTE_NOMBRE_REQUERIDO`, `PAQUETE_RUTA_DIRECCION_REQUERIDA`, `PAQUETE_RUTA_CONTRATO_CATERING_REQUERIDO` (I14) (`Validation`) |
| `RutaId`, `ParadaId`, `RepartidorId`, `PaqueteId`, `PacienteId`, `ContratoId` | `src/Logistica.Domain/Shared/<Nombre>.cs` (6 archivos) | `New()` / `From(Guid)` | `ID_VACIO` (`Validation`), vía `TypedIdValidator.Validar` → `SharedErrors.IdVacio()` en `src/Logistica.Domain/Shared/{TypedIdValidator.cs, SharedErrors.cs}` |

### Enumeraciones (sin lógica propia, no llevan test unitario — UT-13)

`EstadoRuta`, `EstadoEntrega` (`src/Logistica.Domain/Rutas/`), `TipoConstancia`,
`MotivoIncidencia` (`src/Logistica.Domain/Shared/`).

### Read model (fuera de Domain, relevante para Application — §5.7, §9.4)

`PaqueteRecibido` y `EtiquetaPaquete` en
`src/Logistica.Application/PaquetesRecibidos/`: no heredan de `Entity`, no se
acceden por `IRepository<T>`. Su único test unitario es el de mapeo del handler
de integración entrante (sección 4, tabla de Application).

---

## 2. Invariantes I1–I14

| Código | RN | Dónde vive (§6/§6.2) | Código(s) de error | `ErrorType` | Proyecto de test |
|---|---|---|---|---|---|
| I1 | RN-13 | `RutaEntrega.Crear` — Domain | `I1_RUTA_SIN_PARADAS` | Validation | `Logistica.Domain.Tests` |
| I2 | RN-27, RN-13 | Disponibilidad: `Repartidor.AsignarRuta` — Domain. Capacidad: `CrearRutaCommandHandler` — Application | `I2_REPARTIDOR_NO_DISPONIBLE` (mismo código, dos caminos) | Conflict | `Logistica.Domain.Tests` (disponibilidad) + `Logistica.Application.Tests` (capacidad) |
| I3 | RN-13, RN-15 | `RutaEntrega.Optimizar`, `RutaEntrega.Iniciar` — Domain | `I3_RUTA_NO_PENDIENTE`, `I3_RUTA_NO_OPTIMIZADA` | Conflict | `Logistica.Domain.Tests` |
| I4 | RN-16 | Ruta y parada — Domain, en ambos niveles | `I4_TRANSICION_INVALIDA` | Conflict | `Logistica.Domain.Tests` |
| I5 | RN-16 | `ParadaEntrega.ConfirmarEntrega`/`ReportarIncidencia` — Domain | `I5_CONSTANCIA_REQUERIDA`, `I5_INCIDENCIA_REQUERIDA` | Validation | `Logistica.Domain.Tests` |
| I6 | RN-16 | `RutaEntrega.Completar` — Domain | `I6_PARADAS_PENDIENTES` | Conflict | `Logistica.Domain.Tests` |
| I7 | RN-16 | `RutaEntrega.Completar` — Domain | — (determina el estado, no lanza) | — | `Logistica.Domain.Tests` |
| I8 | RN-13, RN-15 | `RutaEntrega.Optimizar` (guard) + `ParadaEntrega.AsignarOrden` — Domain | `I8_ORDEN_PARADAS_INVALIDO` | Validation | `Logistica.Domain.Tests` |
| I9 | RN-17 | `CrearRutaCommandHandler` (unicidad) + `ReponerPaquetesAlCancelarRutaPolicy` (reposición) — Application | `I9_PAQUETE_YA_ASIGNADO` | Conflict | `Logistica.Application.Tests` |
| I10 | RN-10 | `CrearRutaCommandHandler` — Application | `I10_PAQUETES_FECHA_DISTINTA` | Validation | `Logistica.Application.Tests` |
| I11 | RN-27 | `CrearRutaCommandHandler` (comprobación) + `Repartidor.AsignarRuta` (refuerzo) + índice único filtrado (Infrastructure, D-01 §19.1: **no existe hoy**) + políticas de liberación (Application) | `I11_REPARTIDOR_CON_RUTA_ACTIVA` | Conflict | `Logistica.Application.Tests` |
| I12 | §10.5 maestro, RN-17 | `IPaqueteRecibidoStore.UpsertAsync` — Infrastructure. El unit test ejercita el contrato del store a través del fake, en `ProcesarPaquetesListosCommandHandlerTests` | — (el duplicado se ignora) | — | `Logistica.Application.Tests` |
| I13 | RN-13 | `RutaEntrega.Cancelar` — Domain | `I13_RUTA_NO_CANCELABLE`, `I13_MOTIVO_REQUERIDO` | Conflict / Validation | `Logistica.Domain.Tests` |
| I14 | RN-16, RN-23 | Estructural en `PaqueteParaRuta`/`ParadaEntrega` — Domain. Traducción sin transformar — `PublicarEntregaConfirmadaHandler`/`PublicarIncidenciaEntregaRegistradaHandler` — Application. Defensa en profundidad en `ProcesarPaquetesListosCommandHandler` — Application | `PAQUETE_RUTA_PACIENTE_ID_REQUERIDO`, `PAQUETE_RUTA_CONTRATO_CATERING_REQUERIDO` (Domain); `PAQUETE_LISTO_PACIENTE_ID_REQUERIDO`, `PAQUETE_LISTO_CONTRATO_CATERING_REQUERIDO` (Application, declarados en `PaqueteRecibidoErrors`, no en `RutaErrors` — §9.4, no son reutilizados por el agregado) | Validation | Ambos proyectos |

**Nota de verificación:** `RutaErrors` declara además `EVIDENCIA_EXTENSION_NO_PERMITIDA`
y `EVIDENCIA_TAMANO_EXCEDIDO` (`Validation`), lanzados desde
`EvidenciaArchivoValidator` (Application). No tienen invariante `Ixx` — son la
resolución **CERRADA** de `docs/INCOHERENCIAS.md` INC-1 (D-09 §19.2, Opción A) — y
no están en la tabla de `DESIGN.md` §13.1/§13.2 porque esa tabla no se actualizó
tras cerrar INC-1. Se documentan aquí para que el catálogo de errores que use
`test-writer` sea el real, no el desactualizado.

---

## 3. Command handlers, políticas y traductores — ramas de error

### Command handlers sobre `RutaEntrega`

| Handler | Camino feliz | Ramas de error | Código / `ErrorType` | Test en §15.2 |
|---|---|---|---|---|
| `CrearRutaCommandHandler` | Crea la ruta, marca al repartidor no disponible, marca los paquetes `ASIGNADO` | Repartidor inexistente | `REPARTIDOR_NO_ENCONTRADO` / NotFound | Sí |
| | | Repartidor con ruta activa | `I11_REPARTIDOR_CON_RUTA_ACTIVA` / Conflict | Sí |
| | | Paquete ausente del pool `POR_ASIGNAR` | `I9_PAQUETE_YA_ASIGNADO` / Conflict | Sí |
| | | Paquete con fecha distinta a la de la ruta | `I10_PAQUETES_FECHA_DISTINTA` / Validation | Sí |
| | | Vehículo sin capacidad | `I2_REPARTIDOR_NO_DISPONIBLE` / Conflict | Sí |
| `ConfirmarEntregaCommandHandler` | Guarda evidencia, confirma entrega, la URL queda en la constancia | Sin archivo | `EVIDENCIA_ARCHIVO_REQUERIDO` / Validation | Sí |
| | | Extensión no permitida | `EVIDENCIA_EXTENSION_NO_PERMITIDA` / Validation | **Gap** (ver §4.5) |
| | | Tamaño excedido | `EVIDENCIA_TAMANO_EXCEDIDO` / Validation | **Gap** |
| | | Ruta inexistente | `RUTA_NO_ENCONTRADA` / NotFound | **Gap** |
| `ReportarIncidenciaCommandHandler` | Reporta incidencia con o sin foto | Ruta inexistente | `RUTA_NO_ENCONTRADA` / NotFound | **Gap** — suite completa sin test en §15.2 |
| | | Extensión no permitida (si llega archivo) | `EVIDENCIA_EXTENSION_NO_PERMITIDA` / Validation | **Gap** |
| | | Tamaño excedido (si llega archivo) | `EVIDENCIA_TAMANO_EXCEDIDO` / Validation | **Gap** |
| `IniciarRutaCommandHandler` | Delega `Iniciar()` (I3) | Ruta inexistente | `RUTA_NO_ENCONTRADA` / NotFound | No — pass-through trivial, invariante íntegra en Domain (§10.1 DESIGN.md); consistente con el criterio de §13.2 del maestro |
| `OptimizarRutaCommandHandler` | Construye `DireccionGeo`/`Coordenadas` de origen y delega `Optimizar()` (I3, I8) | Ruta inexistente | `RUTA_NO_ENCONTRADA` / NotFound | No — mismo criterio |
| `CompletarRutaCommandHandler` | Delega `Completar()` (I6, I7) | Ruta inexistente | `RUTA_NO_ENCONTRADA` / NotFound | No — mismo criterio |
| `CancelarRutaCommandHandler` | Delega `Cancelar()` (I13) | Ruta inexistente | `RUTA_NO_ENCONTRADA` / NotFound | No — mismo criterio |
| `RegistrarRepartidorCommandHandler` | Delega `Vehiculo.Crear` + `Repartidor.Registrar` | — (validaciones íntegras en Domain) | — | No — ninguna invariante de orquestación (comentario propio del handler) |

### Handler de integración entrante

| Handler | Camino feliz | Ramas de error | Código / `ErrorType` | Test en §15.2 |
|---|---|---|---|---|
| `ProcesarPaquetesListosCommandHandler` | Upsert de cada paquete del lote, `POR_ASIGNAR` | `pacienteId` vacío | `PAQUETE_LISTO_PACIENTE_ID_REQUERIDO` / Validation | Sí |
| | | `contratoCateringId` vacío | `PAQUETE_LISTO_CONTRATO_CATERING_REQUERIDO` / Validation | Sí |
| | Reprocesar el mismo `paqueteId` no duplica (I12) | — | — | Sí |

### Políticas (`INotificationHandler<T>`, §8.2)

| Política | Reacciona a | Efecto | Rama de error | Test en §15.2 |
|---|---|---|---|---|
| `LiberarRepartidorAlCompletarRutaPolicy` | `RutaCompletada` | `Repartidor.Liberar()` | Repartidor inexistente → `REPARTIDOR_NO_ENCONTRADO` / NotFound | Sí |
| `LiberarRepartidorAlCancelarRutaPolicy` | `RutaCancelada` | `Repartidor.Liberar()` | Repartidor inexistente → `REPARTIDOR_NO_ENCONTRADO` / NotFound | Sí |
| `ReponerPaquetesAlCancelarRutaPolicy` | `RutaCancelada` | Marca `POR_ASIGNAR` los `PaqueteIdsNoResueltos` del evento | — | Sí |

### Traductores a evento de integración (§8.3)

| Handler | Evento de dominio → integración | Test en §15.3 |
|---|---|---|
| `PublicarRutaOptimizadaGeneradaHandler` | `RutaOptimizadaGenerada` → `RutaOptimizadaGeneradaIntegrationEvent` (§9.12) | Sí |
| `PublicarEntregaConfirmadaHandler` | `EntregaConfirmada` → `EntregaConfirmadaIntegrationEvent` (§9.13) | Sí (con y sin coordenadas) |
| `PublicarIncidenciaEntregaRegistradaHandler` | `IncidenciaEntregaRegistrada` → `IncidenciaEntregaRegistradaIntegrationEvent` (§9.14) | Sí (con y sin foto) |

Traducción hecha por `DomainToIntegrationMapper` (estático, sin dependencias): se
ejercita a través de los tres handlers de arriba, no por separado.

---

## 4. Una fila por test — §15.1, §15.2, §15.3

### 4.1 Domain — por invariante (§15.1, tabla 1) — 29 tests

| # | Invariante | Test | Origen | Estado |
|---|---|---|---|---|
| 1 | I1 | `Crear_con_lista_vacia_lanza_I1_RUTA_SIN_PARADAS` | DESIGN | |
| 2 | I1 | `Crear_con_paquetes_nace_pendiente_con_una_parada_por_paquete` | DESIGN | |
| 3 | I2 | `AsignarRuta_sobre_no_disponible_lanza_I2_REPARTIDOR_NO_DISPONIBLE` | DESIGN | |
| 4 | I2 | `TieneCapacidadPara_cantidad_igual_a_la_capacidad_es_true` | DESIGN | |
| 5 | I2 | `TieneCapacidadPara_cantidad_mayor_a_la_capacidad_es_false` | DESIGN | |
| 6 | I3 | `Optimizar_sobre_ruta_no_pendiente_lanza_I3_RUTA_NO_PENDIENTE` | DESIGN | |
| 7 | I3 | `Iniciar_sobre_ruta_no_optimizada_lanza_I3_RUTA_NO_OPTIMIZADA` | DESIGN | |
| 8 | I3 | `Iniciar_sobre_ruta_no_pendiente_lanza_I3_RUTA_NO_PENDIENTE` | DESIGN | |
| 9 | I4 | `ConfirmarEntrega_sobre_ruta_no_en_camino_lanza_I4_TRANSICION_INVALIDA` | DESIGN | |
| 10 | I4 | `ReportarIncidencia_sobre_ruta_no_en_camino_lanza_I4_TRANSICION_INVALIDA` | DESIGN | |
| 11 | I4 | `ConfirmarEntrega_sobre_parada_ya_resuelta_lanza_I4_TRANSICION_INVALIDA` | DESIGN | |
| 12 | I5 | `ConfirmarEntrega_con_constancia_pasa_a_ENTREGADO_y_deja_incidencia_nula` | DESIGN | |
| 13 | I5 | `ReportarIncidencia_con_incidencia_pasa_a_NO_ENTREGADO_y_deja_constancia_nula` | DESIGN | |
| 14 | I5 | `ConfirmarEntrega_sobre_ruta_en_camino_deja_Incidencia_en_null_I5` | DESIGN | |
| 15 | I5 | `ReportarIncidencia_sobre_ruta_en_camino_deja_Constancia_en_null_I5` | DESIGN | |
| 16 | I5 | `ConfirmarEntrega_con_constancia_nula_lanza_I5_CONSTANCIA_REQUERIDA` | DESIGN | |
| 17 | I5 | `ReportarIncidencia_con_incidencia_nula_lanza_I5_INCIDENCIA_REQUERIDA` | DESIGN | |
| 18 | I6 | `Completar_con_paradas_pendientes_lanza_I6_PARADAS_PENDIENTES` | DESIGN | |
| 19 | I7 | `Completar_con_todas_ENTREGADO_da_COMPLETADA_I7` | DESIGN | |
| 20 | I7 | `Completar_con_una_NO_ENTREGADO_da_CON_INCIDENCIAS_I7` | DESIGN | |
| 21 | I8 | `Optimizar_con_tres_paradas_asigna_ordenes_1_a_n_sin_repetir_I8` | DESIGN | |
| 22 | I8 | `AsignarOrden_menor_a_uno_lanza_I8_ORDEN_PARADAS_INVALIDO` | DESIGN | |
| 23 | I13 | `Cancelar_con_motivo_vacio_lanza_I13_MOTIVO_REQUERIDO` | DESIGN | |
| 24 | I13 | `Cancelar_ruta_ya_CANCELADA_lanza_I13_RUTA_NO_CANCELABLE` | DESIGN | |
| 25 | I13 | `Cancelar_ruta_COMPLETADA_lanza_I13_RUTA_NO_CANCELABLE` | DESIGN | |
| 26 | I13 | `Cancelar_desde_pendiente_pasa_a_CANCELADA_y_emite_RutaCancelada_con_todos_los_paquetes` | DESIGN | |
| 27 | I14 | `ReportarIncidencia_conserva_PacienteId_y_ContratoCateringId_en_el_evento_I14` | DESIGN | |
| 28 | I14 | `PacienteId_nulo_lanza_PAQUETE_RUTA_PACIENTE_ID_REQUERIDO` (`PaqueteParaRutaTests`) | DESIGN | |
| 29 | I14 | `ContratoCateringId_nulo_lanza_PAQUETE_RUTA_CONTRATO_CATERING_REQUERIDO` (`PaqueteParaRutaTests`) | DESIGN | |

I9, I10, I11, I12 **no** llevan fila aquí a propósito (§6.2): viven en Application, sección 4.3.

### 4.2 Domain — cobertura adicional por suite (§15.1, tabla 2)

**`RutaEntregaOptimizacionTests` — 6 (2 ya en 4.1 + 4 aquí)**

| Test | Origen | Estado |
|---|---|---|
| `Optimizar_calcula_distancia_y_tiempo_estimado_correctos` | DESIGN | |
| `Optimizar_emite_RutaOptimizadaGenerada_con_las_paradas_ordenadas` | DESIGN | |
| `Optimizar_ejecutado_dos_veces_recalcula_desde_cero` | DESIGN | |
| `Optimizar_con_paradas_reales_de_Santa_Cruz_produce_el_orden_esperado` | DESIGN | |

**`ParadaEntregaTests` — 11 (6 ya en 4.1 + 5 aquí)**

| Test | Origen | Estado |
|---|---|---|
| `Nace_pendiente_con_orden_cero_y_direccion_clonada` | DESIGN | |
| `MarcarEnCamino_pasa_de_PENDIENTE_a_EN_CAMINO` | DESIGN | |
| `ReportarIncidencia_sobre_parada_ya_resuelta_lanza_I4_TRANSICION_INVALIDA` (simétrico de la fila 11 de 4.1, que solo cubre `ConfirmarEntrega`) | Derivado | |
| `AsignarOrden_con_valor_valido_fija_Orden` | Derivado | |
| `EstaResuelta_es_false_en_PENDIENTE_o_EN_CAMINO_y_true_en_ENTREGADO_o_NO_ENTREGADO` (`[Theory]`, UT-05) | Derivado | |

**`RepartidorTests` — 10 (3 ya en 4.1 + 7 aquí)**

| Test | Origen | Estado |
|---|---|---|
| `Liberar_es_idempotente_y_no_lanza_si_ya_estaba_disponible` | DESIGN | |
| `Registrar_con_nombre_vacio_lanza_REPARTIDOR_NOMBRE_REQUERIDO` | DESIGN (código) | |
| `Registrar_con_telefono_vacio_lanza_REPARTIDOR_TELEFONO_REQUERIDO` | DESIGN (código) | |
| `Registrar_con_vehiculo_nulo_lanza_REPARTIDOR_VEHICULO_REQUERIDO` | DESIGN (código) | |
| `Registrar_con_datos_validos_crea_el_repartidor_disponible` | Derivado | |
| `AsignarRuta_sobre_disponible_lo_marca_no_disponible` | Derivado | |
| `Liberar_sobre_no_disponible_lo_marca_disponible` | Derivado | |

> **Nota:** 3 (ya contadas en 4.1) + 1 (`Liberar_...idempotente`) + 3 (validaciones
> de `Registrar`) + 3 (derivadas) = 10. Cuadra con el total declarado.

**`PaqueteParaRutaTests` — 6 (2 ya en 4.1 + 4 aquí)**

| Test | Origen | Estado |
|---|---|---|
| `PaqueteId_nulo_lanza_PAQUETE_RUTA_PAQUETE_ID_REQUERIDO` | DESIGN (código) | |
| `PacienteNombre_vacio_lanza_PAQUETE_RUTA_PACIENTE_NOMBRE_REQUERIDO` | DESIGN (código) | |
| `DireccionEntrega_nula_lanza_PAQUETE_RUTA_DIRECCION_REQUERIDA` | DESIGN (código) | |
| `Crear_con_todos_los_campos_validos_construye_el_paquete` | Derivado | |

**`DireccionGeoTests` — 8**

| Test | Origen | Estado |
|---|---|---|
| `Crear_con_calle_vacia_lanza_DIRECCION_CALLE_REQUERIDA` | DESIGN (código) | |
| `Crear_con_zona_vacia_lanza_DIRECCION_ZONA_REQUERIDA` | DESIGN (código) | |
| `Crear_con_ciudad_vacia_lanza_DIRECCION_CIUDAD_REQUERIDA` | DESIGN (código) | |
| `Crear_con_coordenadas_nulas_lanza_DIRECCION_COORDENADAS_REQUERIDAS` (RN-15) | DESIGN (código) | |
| `Referencia_es_opcional` | DESIGN | |
| `DistanciaHasta_el_mismo_punto_es_cero` | DESIGN | |
| `DistanciaHasta_un_grado_de_latitud_en_el_ecuador_da_aproximadamente_111_19_km` | DESIGN | |
| `Clonar_produce_una_instancia_equivalente_pero_con_Coordenadas_propia` | DESIGN | |

**`CoordenadasTests` — 4**

| Test | Origen | Estado |
|---|---|---|
| `Crear_con_latitud_en_el_limite_90_o_menos_90_es_valida` (`[Theory]`) | DESIGN (código) | |
| `Crear_con_latitud_fuera_de_rango_lanza_COORDENADAS_LATITUD_INVALIDA` (`[Theory]`) | DESIGN (código) | |
| `Crear_con_longitud_en_el_limite_180_o_menos_180_es_valida` (`[Theory]`) | DESIGN (código) | |
| `Crear_con_longitud_fuera_de_rango_lanza_COORDENADAS_LONGITUD_INVALIDA` (`[Theory]`) | DESIGN (código) | |

**`ConstanciaEntregaTests` — 4**

| Test | Origen | Estado |
|---|---|---|
| `Crear_con_url_evidencia_vacia_lanza_CONSTANCIA_URL_REQUERIDA` | DESIGN (código) | |
| `Crear_con_receptor_nombre_vacio_lanza_CONSTANCIA_RECEPTOR_REQUERIDO` | DESIGN (código) | |
| `CoordenadasConfirmacion_es_opcional` | DESIGN | |
| `Crear_con_todos_los_campos_validos_construye_la_constancia` | Derivado | |

**`IncidenciaEntregaTests` — 3**

| Test | Origen | Estado |
|---|---|---|
| `Crear_con_descripcion_vacia_lanza_INCIDENCIA_DESCRIPCION_REQUERIDA` | DESIGN (código) | |
| `UrlFoto_es_opcional` | DESIGN | |
| `Crear_con_todos_los_campos_validos_construye_la_incidencia` | Derivado | |

**`VehiculoTests` — 4**

| Test | Origen | Estado |
|---|---|---|
| `Crear_con_tipo_vacio_lanza_VEHICULO_TIPO_REQUERIDO` | DESIGN (código) | |
| `Crear_con_placa_vacia_lanza_VEHICULO_PLACA_REQUERIDA` | DESIGN (código) | |
| `Crear_con_capacidad_menor_o_igual_a_cero_lanza_VEHICULO_CAPACIDAD_INVALIDA` | DESIGN (código) | |
| `Crear_con_datos_validos_construye_el_vehiculo` | Derivado | |

**`TypedIdTests` — 19 (`[Theory]` parametrizado por los 6 IDs donde aplique)**

| Test | Origen | Estado |
|---|---|---|
| `New_genera_un_id_valido` × 6 (uno por `RutaId`, `ParadaId`, `RepartidorId`, `PaqueteId`, `PacienteId`, `ContratoId`) | DESIGN (código) | |
| `From_con_guid_valido_lo_envuelve` × 6 | DESIGN (código) | |
| `From_con_Guid_Empty_lanza_ID_VACIO` × 6 | DESIGN (código) | |
| `Dos_ids_con_el_mismo_Guid_son_iguales_por_valor` (igualdad estructural de `sealed record`) | DESIGN | |

18 + 1 = 19.

### 4.3 Application (§15.2) — 19 tests

**Validaciones que cruzan agregados o consultan repositorio — 8**

| Test | Origen | Estado |
|---|---|---|
| `CrearRutaCommandHandlerTests.Handle_repartidor_sin_capacidad_lanza_I2_REPARTIDOR_NO_DISPONIBLE` | DESIGN | |
| `CrearRutaCommandHandlerTests.Handle_paquete_no_esta_POR_ASIGNAR_lanza_I9_PAQUETE_YA_ASIGNADO` | DESIGN | |
| `CrearRutaCommandHandlerTests.Handle_paquete_con_fecha_distinta_lanza_I10_PAQUETES_FECHA_DISTINTA` | DESIGN | |
| `CrearRutaCommandHandlerTests.Handle_repartidor_con_ruta_activa_lanza_I11_REPARTIDOR_CON_RUTA_ACTIVA` | DESIGN | |
| `CrearRutaCommandHandlerTests.Handle_repartidor_inexistente_lanza_REPARTIDOR_NO_ENCONTRADO` | DESIGN | |
| `CrearRutaCommandHandlerTests.Handle_caso_feliz_crea_la_ruta_asigna_al_repartidor_y_marca_los_paquetes` | DESIGN | |
| `ConfirmarEntregaHandlerTests.Handle_sin_archivo_lanza_EVIDENCIA_ARCHIVO_REQUERIDO` | DESIGN | |
| `ConfirmarEntregaHandlerTests.Handle_con_archivo_guarda_la_evidencia_y_la_url_queda_en_la_constancia` | DESIGN | |

**Idempotencia del endpoint de integración — 5**

| Test | Origen | Estado |
|---|---|---|
| `ProcesarPaquetesListosCommandHandlerTests.Handle_reprocesar_el_mismo_paqueteId_no_duplica_I12` | DESIGN | |
| `ProcesarPaquetesListosCommandHandlerTests.Handle_paquete_con_PacienteId_vacio_lanza_PAQUETE_LISTO_PACIENTE_ID_REQUERIDO` | DESIGN | |
| `ProcesarPaquetesListosCommandHandlerTests.Handle_paquete_con_ContratoCateringId_vacio_lanza_PAQUETE_LISTO_CONTRATO_CATERING_REQUERIDO` | DESIGN | |
| `ProcesarPaquetesListosCommandHandlerTests.Handle_paquete_valido_upsertea_un_PaqueteRecibido_mapeado_correctamente` | DESIGN | |
| `ProcesarPaquetesListosCommandHandlerTests.Handle_procesa_todos_los_paquetes_del_lote` | DESIGN | |

**Políticas — 6**

| Test | Origen | Estado |
|---|---|---|
| `LiberarRepartidorAlCompletarRutaPolicyTests.Handle_libera_al_repartidor_de_la_ruta_completada` | DESIGN | |
| `LiberarRepartidorAlCompletarRutaPolicyTests.Handle_sin_repartidor_lanza_REPARTIDOR_NO_ENCONTRADO` | DESIGN | |
| `LiberarRepartidorAlCancelarRutaPolicyTests.Handle_libera_al_repartidor_de_la_ruta_cancelada` | DESIGN | |
| `LiberarRepartidorAlCancelarRutaPolicyTests.Handle_sin_repartidor_lanza_REPARTIDOR_NO_ENCONTRADO` | DESIGN | |
| `ReponerPaquetesAlCancelarRutaPolicyTests.Handle_marca_por_asignar_los_paquetes_no_resueltos` | DESIGN | |
| `ReponerPaquetesAlCancelarRutaPolicyTests.Handle_sin_paquetes_no_resueltos_no_marca_nada` | DESIGN | |

### 4.4 Contratos de eventos (§15.3) — 6 tests

| Test | Origen | Estado |
|---|---|---|
| `PublicarRutaOptimizadaGeneradaHandlerTests.Handle_publica_RutaOptimizadaGeneradaIntegrationEvent_con_el_payload_de_SISTEMA_8_12` | DESIGN | |
| `PublicarEntregaConfirmadaHandlerTests.Handle_publica_EntregaConfirmadaIntegrationEvent_con_el_payload_de_SISTEMA_8_13` | DESIGN | |
| `PublicarEntregaConfirmadaHandlerTests.Handle_sin_coordenadas_de_confirmacion_publica_CoordenadasConfirmacion_null` | DESIGN | |
| `PublicarIncidenciaEntregaRegistradaHandlerTests.Handle_publica_IncidenciaEntregaRegistradaIntegrationEvent_con_el_payload_de_SISTEMA_8_14` | DESIGN | |
| `PublicarIncidenciaEntregaRegistradaHandlerTests.Handle_sin_foto_publica_UrlFoto_null` | DESIGN | |
| `ProcesarPaquetesListosCommandHandlerTests.Handle_paquete_valido_upsertea_un_PaqueteRecibido_mapeado_correctamente` (mismo test que en 4.3 — el DTO de entrada **es** el contrato, §11.8 DESIGN.md; no se duplica en el conteo de la suite) | DESIGN | |

### 4.5 Gap detectado contra el código real (no listado en `DESIGN.md` §15.2)

No cuenta para el recuento de la sección 5 (es adicional a lo que las tablas
piden), pero **sí debe generarse** para que la cobertura ≥ 80 % (§15.6) no
dependa de código sin ejercitar. Se recomienda incorporarlo a `DESIGN.md` §15.2
en una futura revisión.

| Test propuesto | Handler | Código / `ErrorType` | Estado |
|---|---|---|---|
| `ConfirmarEntregaHandlerTests.Handle_ruta_inexistente_lanza_RUTA_NO_ENCONTRADA` | `ConfirmarEntregaCommandHandler` | `RUTA_NO_ENCONTRADA` / NotFound | |
| `ConfirmarEntregaHandlerTests.Handle_extension_no_permitida_lanza_EVIDENCIA_EXTENSION_NO_PERMITIDA` | `ConfirmarEntregaCommandHandler` | `EVIDENCIA_EXTENSION_NO_PERMITIDA` / Validation | |
| `ConfirmarEntregaHandlerTests.Handle_archivo_excede_el_tamano_maximo_lanza_EVIDENCIA_TAMANO_EXCEDIDO` | `ConfirmarEntregaCommandHandler` | `EVIDENCIA_TAMANO_EXCEDIDO` / Validation | |
| `ReportarIncidenciaHandlerTests.Handle_ruta_inexistente_lanza_RUTA_NO_ENCONTRADA` | `ReportarIncidenciaCommandHandler` | `RUTA_NO_ENCONTRADA` / NotFound | |
| `ReportarIncidenciaHandlerTests.Handle_extension_no_permitida_lanza_EVIDENCIA_EXTENSION_NO_PERMITIDA` | `ReportarIncidenciaCommandHandler` | `EVIDENCIA_EXTENSION_NO_PERMITIDA` / Validation | |
| `ReportarIncidenciaHandlerTests.Handle_archivo_excede_el_tamano_maximo_lanza_EVIDENCIA_TAMANO_EXCEDIDO` | `ReportarIncidenciaCommandHandler` | `EVIDENCIA_TAMANO_EXCEDIDO` / Validation | |
| `ReportarIncidenciaHandlerTests.Handle_con_foto_guarda_la_evidencia_y_la_url_queda_en_la_incidencia` | `ReportarIncidenciaCommandHandler` | caso feliz, con foto | |
| `ReportarIncidenciaHandlerTests.Handle_sin_foto_registra_la_incidencia_con_UrlFoto_null` | `ReportarIncidenciaCommandHandler` | caso feliz, sin foto | |

---

## 5. Recuento de filas

| Sección | Filas |
|---|---|
| 4.1 Domain — por invariante | 29 |
| 4.2 Domain — cobertura adicional | 62 (6+11+10+6+8+4+4+3+4+19 − 13 ya contadas en 4.1) |
| **Domain, total** | **91** |
| 4.3 Application | 19 |
| 4.4 Contratos | 6 (5 filas propias + 1 compartida con Application, no duplicada) |
| **Total reconciliado con `DESIGN.md` §15.1–15.3** | **116** |
| 4.5 Gap (adicional, no pedido por `DESIGN.md`) | 8 |

`DESIGN.md` (introducción a §15) dice **«123 casos»**. La reconciliación fila por
fila da **116**. La diferencia (7) no se pudo atar a un test con nombre o código
explícito en el texto de `DESIGN.md` — probablemente son variantes de `[Theory]`
no desglosadas en la prosa (p. ej. más casos límite de `TypedIdTests` o de
`CoordenadasTests`). No es una incoherencia bloqueante: no contradice ninguna
regla de negocio ni cambia dónde vive una invariante, así que no se registra en
`docs/INCOHERENCIAS.md`; se deja anotado para que quien cierre la fase 9 sepa que
el «123» de la prosa es aproximado.

---

## 6. Fakes y builders (§15.2)

Todos en `TestSupport/` de `Logistica.Application.Tests` (UT-12), salvo que se
indique lo contrario.

| Nombre | Rol | Notas |
|---|---|---|
| `RutaEntregaRepositoryFake` | Implementa `IRutaEntregaRepository` en memoria (`GetByIdAsync`, `AddAsync` de `IRepository<T>`) | — |
| `RepartidorRepositoryFake` | Implementa `IRepartidorRepository`, incluido `TieneRutaActivaAsync` | El fake decide el resultado de `TieneRutaActivaAsync` por configuración explícita del test, no consultando una lista de rutas real |
| `PaqueteRecibidoStoreFake` | Implementa `IPaqueteRecibidoStore` en memoria: `UpsertAsync` (con la semántica de I12 — ignora duplicado), `ObtenerPorAsignarAsync`, `ObtenerPorIdsAsync`, `MarcarAsignadosAsync`, `MarcarPorAsignarAsync`, `ConsultarAsync` | El único store con lógica propia que probar (I12): el fake debe reproducir el "ignora, no sobrescribe" real, no solo servir de doble tonto |
| `UnitOfWorkFake` | Implementa `IUnitOfWork`; expone si `CommitAsync` se llamó, para las aserciones "sin efectos" de UT-06 | — |
| `EvidenciaStorageFake` | Implementa `IEvidenciaStorage.GuardarAsync`, devuelve una URL fija configurable | — |
| `IntegrationEventPublisherFake` | Implementa `IIntegrationEventPublisher.PublishAsync`, captura lo publicado para aserciones de §15.3 | — |

**Builders** (`TestSupport/` de cada proyecto, uno por objetivo con más de 2-3
parámetros obligatorios, siguiendo UT-12): `RutaEntregaBuilder`,
`ParadaEntregaBuilder` (vía `RutaEntregaBuilder`, dado que el constructor de
`ParadaEntrega` es `internal`), `RepartidorBuilder`, `VehiculoBuilder`,
`DireccionGeoBuilder`, `PaqueteParaRutaBuilder`, `PaqueteRecibidoBuilder` (para
`Logistica.Application.Tests`).

---

## 7. Orden de ataque para `/unit-tests todo`

1. **VOs**: `Coordenadas` → `DireccionGeo` → `Vehiculo` → `ConstanciaEntrega` →
   `IncidenciaEntrega` → `PaqueteParaRuta` → los seis IDs tipados (`TypedIdTests`).
2. **Entidad hija y agregados**: `ParadaEntrega` → `RutaEntrega`
   (`RutaEntregaInvariantesTests`, luego `RutaEntregaOptimizacionTests`) →
   `Repartidor`.
3. **Handlers y políticas**: `CrearRutaCommandHandler` → `ConfirmarEntregaCommandHandler`
   (incluidos los tests de la sección 4.5) → `ReportarIncidenciaCommandHandler`
   (sección 4.5 completa) → `ProcesarPaquetesListosCommandHandler` → las tres
   políticas.
4. **Contratos de eventos**: los tres traductores de §8.3.

Cada paso depende del anterior solo en el sentido de que un VO sin test no debería
bloquear al agregado que lo usa (los fakes/builders ya construyen instancias
válidas); el orden es el de menor a mayor dependencia, no una barrera dura.
