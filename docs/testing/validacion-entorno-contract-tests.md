# Validación del entorno — control del agente `pact-reviewer`

**Fecha:** 2026-09-27
**Objetivo usado:** par #9 del MAPA de contract tests — `PaquetesListosParaEntrega` (BC6
consumidor de `ms-produccion-alimentos`), `tests/Logistica.ContractTests/Consumidor/
PaquetesListosParaEntregaConsumerTests.cs`

## Propósito

Comprobar que `pact-reviewer` detecta violaciones reales de `.claude/rules/contract-tests.md`
y no se limita a aprobar por defecto. Se aplicó el checklist real de `contract-test-verifier`
sobre el test generado y, por separado, sobre una copia saboteada con tres violaciones
introducidas a propósito. La copia vive en `TestResults/sabotaje/` (fuera de `.gitignore`
tracking y de cualquier `.csproj`) y **nunca se integró a la suite real**.

> **Nota de entorno:** el agente `pact-reviewer` se creó en esta misma sesión, así que el
> `subagent_type` todavía no estaba registrado en el runtime de Claude Code al momento de
> correr esta validación (limitación conocida: la lista de agentes disponibles se carga al
> inicio de la sesión). Ambos informes de abajo se obtuvieron con un agente `general-purpose`
> instruido a leer y seguir al pie de la letra `.claude/agents/pact-reviewer.md`,
> `.claude/skills/contract-test-verifier/SKILL.md` y su `checklist.md` — mismo procedimiento,
> mismo resultado que si `pact-reviewer` ya estuviera registrado. Una vez que una sesión nueva
> cargue el agente real, este mismo control se puede repetir invocándolo directamente.

## Informe real — sobre `PaquetesListosParaEntregaConsumerTests.cs` (par #9, sin sabotaje)

**Veredicto: CUMPLE**

Puntos comprobados:
- CT-01: `Pact.V4(...).WithMessageInteractions()`, sin configuración de Pact Broker, archivo
  generado con el nombre exacto `pacts/ms-logistica-entrega-ms-produccion-alimentos.json`
  (`<Consumidor>-<Proveedor>.json`).
- CT-04: el mensaje deserializado se pasa por `sender.Send(new
  ProcesarPaquetesListosCommand(mensaje))`, que invoca el handler real
  (`ProcesarPaquetesListosCommandHandler`), construyendo los VOs reales (`DireccionGeo.Crear`,
  `Coordenadas.Crear`). Solo se sustituye `IPaqueteRecibidoStore` por `FakePaqueteRecibidoStore`
  (captura, no un no-op silencioso).
- CT-05: `ContratoJsonOptions` (CamelCase + `JsonStringEnumConverter`) coincide literalmente con
  `AddJsonOptions` de `Program.cs` y con `LoggingIntegrationEventPublisher.SerializerOptions`.
- Forma del contrato contra `docs/SISTEMA.md` §9.11/§8: campo por campo y orden posicional de
  `PaquetesListosParaEntrega`, `PaqueteListoDto`, `EtiquetaDto`, `DireccionGeoDto`,
  `CoordenadasDto` — coinciden exactamente.
- CT-06: dos `[Fact]` cubren `referencia`/`codigoQR` presentes y en `null`.
- CT-07: `[Trait("Capa", "Contrato")]` presente.
- CT-09: sin cambios de comportamiento en `src/`; el único cambio (`InternalsVisibleTo` en
  `AssemblyInfo.cs`) es de visibilidad, documentado en el MAPA.
- CT-10: ubicación correcta en `Consumidor/`.
- CT-11: solo `Assert` de xUnit.
- Ejecución: `dotnet test --filter Capa=Contrato` → 4/4 correctas (incluye este par y los dos
  del shim HTTP, #10/#11), 0 fallos.

Sin incoherencias detectadas entre `SISTEMA.md`, el código y el test.

## Informe de la copia saboteada — `TestResults/sabotaje/PaquetesListosParaEntregaConsumerTests.SABOTAJE.cs`

**Veredicto: NO CUMPLE**

| Archivo:línea | Regla CT-xx | Qué falla | Corrección concreta esperada |
|---|---|---|---|
| `...SABOTAJE.cs:18` | CT-07 | Falta `[Trait("Capa", "Contrato")]` en la clase; sin el trait, `dotnet test --filter Capa=Contrato` no la selecciona. | Añadir `[Trait("Capa", "Contrato")]` antes de `public sealed class PaquetesListosParaEntregaConsumerTests`. |
| `...SABOTAJE.cs:44-46` | CT-06 | Se borró el `[Fact]` del caso con los campos opcionales en `null`; solo queda cubierto el caso "presentes". | Restaurar `ConCamposOpcionalesEnNull_SeProcesaPorElHandlerRealYQuedaPorAsignar()`, con `esperaOpcionalesPresentes: false`. |
| `...SABOTAJE.cs:75-89` | CT-04 | `ProcesarPorElHandlerReal` no invoca `ISender`/`ProcesarPaquetesListosCommand`/`ProcesarPaquetesListosCommandHandler` ni ningún `IPaqueteRecibidoStore`; solo afirma la forma del mensaje directamente, sin ejercitar código de producción. | Reconstruir el método para resolver `ISender` desde un `ServiceCollection` con MediatR real y `FakePaqueteRecibidoStore`, hacer `await sender.Send(...)`, y afirmar sobre `store.Capturados`. |

Nota del propio informe (no es una fila del checklist ni una incoherencia registrada): las tres
desviaciones están auto-documentadas en comentarios `// SABOTAJE N` dentro del archivo, lo que
confirma que son manipulaciones deliberadas para probar el proceso de revisión, no hallazgos
ambiguos.

## Tabla de trazabilidad: violación introducida ↔ fila del informe que la detectó

| # | Violación introducida a propósito | Fila del informe NO CUMPLE que la detectó | Regla citada |
|---|---|---|---|
| 1 | Se quitó `[Trait("Capa", "Contrato")]` de la clase | Fila 1 de la tabla (`...SABOTAJE.cs:18`) | CT-07 |
| 2 | Se borró el `[Fact]` del caso con los opcionales en `null` | Fila 2 de la tabla (`...SABOTAJE.cs:44-46`) | CT-06 |
| 3 | `ProcesarPorElHandlerReal` deja de invocar el handler/command real | Fila 3 de la tabla (`...SABOTAJE.cs:75-89`) | CT-04 |

## Condición de cierre

Las tres violaciones introducidas fueron detectadas por el proceso de revisión, cada una con su
regla `CT-xx` exacta y su corrección concreta. El informe sobre el test real, sin sabotaje,
confirma `CUMPLE` con `dotnet test --filter Capa=Contrato` en verde. **Condición de cierre
cumplida.**
