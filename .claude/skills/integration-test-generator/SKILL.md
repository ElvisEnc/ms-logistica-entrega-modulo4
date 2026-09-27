---
name: integration-test-generator
description: >
  Genera integration tests de un flujo del MAPA de pruebas de integración,
  siguiendo .claude/rules/integration-tests.md. Usar al escribir la clase de
  test de un flujo (WebApplicationFactory + Testcontainers.PostgreSql), o al
  corregir tests tras un informe de revisión.
---

# integration-test-generator

Genérica: no nombra flujos, endpoints ni códigos de error de ningún
microservicio en particular. Todo dato específico se lee en tiempo de
ejecución de `docs/DESIGN.md` y `docs/testing/MAPA-INTEGRATION-TESTS.md` del
repositorio donde se esté trabajando.

## Procedimiento

1. Recibe un flujo tal como aparece en `docs/testing/MAPA-INTEGRATION-TESTS.md`
   — o, si llega un informe de revisión, la lista de filas que señala.
2. Lee `.claude/rules/integration-tests.md` completo y localiza la fila o filas
   del flujo en el MAPA.
3. Ubica los endpoints y handlers reales del flujo con
   `mcp__codegraph__codegraph_explore` si está disponible; si no, `Grep`/`Glob`.
4. Lee el código real y lo que le corresponde en `docs/DESIGN.md` §11 (API
   REST), §12 (el flujo concreto: pasos, cuerpos, HTTP) y §13 (catálogo de
   errores).
5. **Antes de escribir ningún test**, presenta una tabla: camino → regla
   `IT-xx` → HTTP/`codigo` esperado. Cubre el camino correcto y cada camino
   incorrecto que el MAPA liste para ese flujo.
6. Escribe la clase de test del flujo en la ubicación que declare
   `.claude/rules/integration-tests.md` (IT-10), reutilizando o completando los
   helpers de `tests/<Proyecto>.IntegrationTests/Setup/` que falten (el
   `WebApplicationFactory` con `Testcontainers.PostgreSql` y la sustitución del
   publicador de eventos por uno que captura, IT-01/IT-04).
7. Corre `dotnet test --filter Flujo=F<n>-...` del proyecto de integración
   hasta que esté en verde.
8. Actualiza la columna «Estado» de la fila o filas del MAPA (`Hecho`, o
   `INC-<n>` si aplica).
9. Devuelve la lista de archivos creados o modificados.

## Si el código de `src/` no cumple `DESIGN.md`

No se toca `src/`. El test se deja escrito con `[Fact(Skip = "INC-<n>")]`
(IT-09) y se aplica el protocolo de incoherencias de `CLAUDE.md`. Antes de
registrar la incoherencia, se revisa `docs/DESIGN.md` §19: si coincide con una
desviación `D-xx` ya documentada, la entrada la cita en vez de duplicarla. La
columna «Estado» de esa fila queda en `INC-<n>`.

## Si se recibe un informe `Veredicto: NO CUMPLE`

Se corrigen **solo** las filas que el informe señala (`Archivo:línea`, regla
`IT-xx`) y se vuelve a correr `dotnet test`. No se reescribe ningún test que el
informe no haya señalado.
