# MAPA de integration tests — `ms-logistica-entrega`

Inventario específico de este repositorio que alimenta `/integration-tests`. Fuente: `docs/DESIGN.md`
§11 (API REST), §12.1–§12.3 (los tres flujos), §13 (catálogo de errores) y §15.4 (tabla de flujos).
Los endpoints, handlers y `Error` citados aquí se verificaron contra el código real con
`mcp__codegraph__codegraph_explore` (no solo contra la prosa de `DESIGN.md`); no se encontró ninguna
discrepancia entre ambos.

**Notación de origen** (igual convención que `MAPA-UNIT-TESTS.md`): `DESIGN` = nombre/código literal
en `DESIGN.md`; `DESIGN (código)` = solo el código de error, verificado contra `RutaErrors`/
`PaqueteRecibidoErrors` reales; `Derivado` = ruta/nombre de handler confirmado por CodeGraph que
`DESIGN.md` no deletrea literalmente.

## 1. Los tres flujos y su recorrido real

| Flujo | HU / RN | Recorrido HTTP (en orden) | Handlers reales |
|---|---|---|---|
| **F1-RutaOptimizada** | HU-37 + HU-38 | `POST /api/integracion/paquetes-listos` (precondición: pool `POR_ASIGNAR`) → `POST /api/rutas` → `POST /api/rutas/{id}/optimizar` | `ProcesarPaquetesListosCommandHandler`, `CrearRutaCommandHandler`, `OptimizarRutaCommandHandler` |
| **F2-ConfirmacionDeEntrega** | HU-40 | Precondición hasta `EN_CAMINO`: …(igual que F1) → `POST /api/rutas/{id}/iniciar` → `POST /api/rutas/{rutaId}/paradas/{paradaId}/confirmar` (multipart) | `IniciarRutaCommand` (RutasController), `ConfirmarEntregaCommandHandler` |
| **F3-Incidencia** | HU-41 + RN-23 | Precondición hasta `EN_CAMINO`: igual que F2 → `POST /api/rutas/{rutaId}/paradas/{paradaId}/incidencia` (multipart) | `ReportarIncidenciaCommandHandler` |

La precondición común (repartidor disponible + paquete `POR_ASIGNAR` con la fecha correcta) se
construye con los endpoints reales (`POST /api/repartidores`, `POST /api/integracion/paquetes-listos`),
no escribiendo directamente en el `DbContext`: son integration tests de caja negra sobre la API.

## 2. Filas — una por camino (correcto e incorrecto)

| # | Flujo | Camino | HTTP esperado | `codigo` esperado | Efecto a afirmar | Origen | Estado |
|---|---|---|---|---|---|---|---|
| 1 | F1-RutaOptimizada | Correcto | 200 en `optimizar` | — | `RutaOptimizadaGeneradaIntegrationEvent` capturado: `RutaId`, `RepartidorId`, `Fecha`, `Paradas` ordenadas por `Orden` (record real de 4 campos, `Application/IntegrationEvents/RutaOptimizadaGeneradaIntegrationEvent.cs`) | DESIGN §15.4 + Derivado (payload) | |
| 2 | F1-RutaOptimizada | Incorrecto — repartidor con ruta activa | 409 en `POST /api/rutas` | `I11_REPARTIDOR_CON_RUTA_ACTIVA` | El repartidor conserva su ruta original; no se crea una segunda ruta (`GET /api/rutas?repartidorId=&fecha=` de la primera ruta sin cambios) | DESIGN §15.4 + DESIGN (código) `RutaErrors.RepartidorConRutaActiva` | |
| 3 | F1-RutaOptimizada | Incorrecto — repartidor inexistente | 404 en `POST /api/rutas` | `REPARTIDOR_NO_ENCONTRADO` | No se crea ninguna ruta; el paquete sigue `POR_ASIGNAR` en `GET /api/paquetes` | DESIGN §15.4 + DESIGN (código) `RepartidorErrors.NoEncontrado` | |
| 4 | F2-ConfirmacionDeEntrega | Correcto | 200 en `confirmar` | — | Parada queda `ENTREGADO`, `Constancia` asignada e `Incidencia = null` (I5); `EntregaConfirmadaIntegrationEvent` capturado con `Constancia` **anidada** (`ConstanciaDto`), `coordenadasConfirmacion` presente aunque valga `null` si no llegaron lat/lon | DESIGN §15.4 + Derivado (payload, 6 campos con `ConstanciaDto` anidado) | |
| 5 | F2-ConfirmacionDeEntrega | Incorrecto — confirmar dos veces la misma parada | 409 en el segundo `confirmar` | `I4_TRANSICION_INVALIDA` | La constancia original **no se sobrescribe**: releer la parada (`GET /api/rutas/{id}`) muestra la misma `urlEvidencia`/`receptorNombre` del primer intento | DESIGN §15.4 + DESIGN (código) `RutaErrors.TransicionInvalida` | |
| 6 | F3-Incidencia | Correcto | 200 en `incidencia` | — | Parada queda `NO_ENTREGADO`, `Incidencia` asignada y `Constancia = null` (I5); `IncidenciaEntregaRegistradaIntegrationEvent` capturado con **10 campos**, incluido `UrlFoto` presente con valor `null` cuando no se adjunta archivo | DESIGN §15.4 + Derivado (payload, 10 campos, `Application/IntegrationEvents/IncidenciaEntregaRegistradaIntegrationEvent.cs`) | |
| 7 | F3-Incidencia | Incorrecto — incidencia sobre parada ya confirmada | 409 en `incidencia` | `I4_TRANSICION_INVALIDA` | La parada sigue `ENTREGADO` con su constancia original; no se le asigna una incidencia | DESIGN §15.4 + DESIGN (código) `RutaErrors.TransicionInvalida` | |

Recuento: 3 flujos, 7 filas (4 correctas/derivadas + 3 incorrectas — F1 lleva dos caminos
incorrectos porque `DESIGN.md` §15.4 documenta los dos y ninguno sustituye al otro). Cuadra con el
mínimo de `DESIGN.md` §15.4 (2 caminos por flujo) y lo supera en F1.

## 3. Orden de ataque para `/integration-tests todo`

1. **F2-ConfirmacionDeEntrega** primero (fila 4): es el flujo más simple de fixture (una sola
   parada, sin rama de optimización) y sirve de validación de entorno (Prompt 5).
2. **F3-Incidencia** (filas 6–7): reutiliza la misma precondición que F2 hasta `EN_CAMINO`.
3. **F1-RutaOptimizada** (filas 1–3): el único que cubre el recorrido completo desde
   `paquetes-listos` hasta `optimizar`, con dos caminos incorrectos.

## 4. Fixtures y helpers necesarios

- `LogisticaWebApplicationFactory` (`tests/Logistica.IntegrationTests/Setup/`, ya creada en el
  Prompt 1): arranca `Testcontainers.PostgreSql` y aplica migraciones. **Falta añadir** en el
  `ConfigureWebHost` de las clases de flujo: sustitución de `IIntegrationEventPublisher` por un
  publicador que captura (IT-04), reutilizable entre los tres flujos.
- Helpers HTTP para la precondición común (crear repartidor, enviar `paquetes-listos`, crear ruta,
  optimizar, iniciar) — se crean en `tests/Logistica.IntegrationTests/Setup/` la primera vez que un
  flujo los necesite, y los flujos siguientes los reutilizan.
