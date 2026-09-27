# Mapa de contract tests — `ms-logistica-entrega` (BC6)

Inventario específico de este repositorio para `/contract-tests`. Fuentes: `docs/SISTEMA.md`
§13.5 (tabla de los tres pares de BC6) y §9.11/§9.13/§9.14 (forma exacta de cada evento, campos
opcionales incluidos), `docs/DESIGN.md` §15.5, y `docs/INCOHERENCIAS.md` INC-2 (bug verificado en
PactNet 5 que bloquea la verificación de proveedor en modo mensaje — resuelto con la opción B:
Pact HTTP sobre un shim de solo test para los pares donde BC6 es proveedor).

## Código real localizado

| Pieza | Ubicación |
|---|---|
| Traductor de dominio a integración | `src/Logistica.Application/Rutas/IntegrationEventHandlers/DomainToIntegrationMapper.cs` (`internal static`, invocado vía MediatR — no necesita `InternalsVisibleTo`) |
| Handlers que publican (uno por evento saliente) | `src/Logistica.Application/Rutas/IntegrationEventHandlers/Publicar{EntregaConfirmada,IncidenciaEntregaRegistrada,RutaOptimizadaGenerada}Handler.cs` |
| Puerto de publicación | `src/Logistica.Application/Abstractions/IIntegrationEventPublisher.cs` |
| Publicador real (única implementación) | `src/Logistica.Infrastructure/Messaging/LoggingIntegrationEventPublisher.cs` — `JsonSerializerOptions`: `PropertyNamingPolicy = CamelCase` + `JsonStringEnumConverter()` (sin naming policy propia: los enums ya están en `SCREAMING_SNAKE_CASE`) |
| Endpoint consumidor (evento entrante) | `POST /api/integracion/paquetes-listos` en `src/Logistica.WebApi/Controllers/IntegracionController.cs`, despacha `ProcesarPaquetesListosCommand` |
| Handler del evento entrante | `src/Logistica.Application/PaquetesRecibidos/ProcesarPaquetesListosCommandHandler.cs` (`internal`), depende de `IPaqueteRecibidoStore` (sustituible por un fake que captura, CT-04) |
| Assembly marker para `AddMediatR` | `typeof(Logistica.Application.Abstractions.IIntegrationEventPublisher).Assembly` (público; los handlers `internal` los descubre MediatR por reflexión) |
| Visibilidad ampliada en `src/` (único cambio, análogo a `public partial class Program {}` de la Tarea 2) | `src/Logistica.Application/AssemblyInfo.cs` — se añadió `[assembly: InternalsVisibleTo("Logistica.ContractTests")]` para poder construir `ProcesarPaquetesListosCommand` (record `internal`) desde el test del par #9; es visibilidad, no comportamiento |

## Los tres pares de BC6 (`SISTEMA.md` §13.5)

| # | Rol de BC6 | Contraparte | Evento(s) | Transporte |
|---|---|---|---|---|
| 9 | Consumidor | `ms-produccion-alimentos` | `PaquetesListosParaEntrega` | Message pact (PactNet 5) — sin bloqueo, funciona |
| 10 | Proveedor | `ms-pacientes` | `EntregaConfirmada`, `IncidenciaEntregaRegistrada` | **Pact HTTP** sobre shim de solo test (CT-02) — bootstrap, INC-2 opción B |
| 11 | Proveedor | `ms-catering` | `IncidenciaEntregaRegistrada` | **Pact HTTP** sobre shim de solo test (CT-02) — bootstrap, INC-2 opción B |

## Filas por interacción

| Fila | Par | Evento | Rol de BC6 | Campos con matcher | Caso opcional | Pacto | Estado |
|---|---|---|---|---|---|---|---|
| 1 | #9 | `PaquetesListosParaEntrega` | Consumidor (real) | `fecha` (DateOnly ISO), `paquetes[].paqueteId/pacienteId/pacienteNombre/contratoCateringId` (Guid/string), `paquetes[].direccionEntrega.*`, `paquetes[].etiqueta.*` | `referencia` y `codigoQR` **presentes** | `pacts/ms-logistica-entrega-ms-produccion-alimentos.json` | Hecho |
| 2 | #9 | `PaquetesListosParaEntrega` | Consumidor (real) | igual que fila 1 | `referencia` y `codigoQR` en **null** | `pacts/ms-logistica-entrega-ms-produccion-alimentos.json` | Hecho |
| 3 | #10 | `EntregaConfirmada` | Proveedor (bootstrap) | `rutaId/paradaId/paqueteId/pacienteId` (Guid), `pacienteNombre` (string), `constancia.tipo` (enum SCREAMING_SNAKE_CASE), `constancia.urlEvidencia/receptorNombre/fechaHora`, `constancia.coordenadasConfirmacion` | `coordenadasConfirmacion` presente | `pacts/ms-pacientes-ms-logistica-entrega.json` | Hecho |
| 4 | #10 | `IncidenciaEntregaRegistrada` | Proveedor (bootstrap) | `rutaId/paradaId/paqueteId/pacienteId/contratoCateringId` (Guid), `pacienteNombre` (string), `motivo` (enum), `descripcion/fechaHora`, `urlFoto` | `urlFoto` en **null** (el caso con valor lo cubre §15.3, ya generado en Tarea 1) | `pacts/ms-pacientes-ms-logistica-entrega.json` | Hecho |
| 5 | #11 | `IncidenciaEntregaRegistrada` | Proveedor (bootstrap) | igual que fila 4 | `urlFoto` en **null** | `pacts/ms-catering-ms-logistica-entrega.json` | Hecho |

Mínimo de la actividad (SISTEMA.md §13.5: «un par consumidor-proveedor con dos interacciones
verificadas») cubierto por las filas 3-4 (par #10, BC6 proveedor verificado dos veces contra el
shim real — `dotnet test --filter Capa=Contrato` en verde).

## Shim de solo test (CT-02/CT-03)

`tests/Logistica.ContractTests/Testing/PactProviderHost.cs`: host HTTP mínimo (Kestrel real, no
`WebApplicationFactory` — PactNet exige un socket TCP real para la verificación HTTP, no un
`TestServer` en memoria). Registra `AddMediatR` desde el ensamblado de `Logistica.Application`
(descubre los handlers `internal` por reflexión, sin necesitar `InternalsVisibleTo`) y sustituye
`IIntegrationEventPublisher` por `CapturingIntegrationEventPublisher` (`Testing/
CapturingIntegrationEventPublisher.cs`). Expone dos endpoints POST de solo test
(`/pact-provider/entrega-confirmada`, `/pact-provider/incidencia`) que: reciben el escenario
(qué caso opcional), construyen el evento de dominio real con los VOs reales, lo publican con
`IPublisher.Publish` (ejecuta el handler y el traductor reales, sin cambios), y devuelven lo
capturado serializado con las `JsonSerializerOptions` copiadas literalmente de
`LoggingIntegrationEventPublisher`. No toca `src/`.

## Orden de ataque para `/contract-tests todo`

1. **#10** (filas 3-4) — mínimo viable de la actividad, y el que ejercita el shim HTTP completo.
2. **#11** (fila 5) — reutiliza el mismo shim, una sola interacción más.
3. **#9** (filas 1-2) — no depende del shim ni de INC-2; se puede generar en cualquier momento,
   incluso antes de resolver INC-2.

Un par bloqueado no impide avanzar con los demás.
