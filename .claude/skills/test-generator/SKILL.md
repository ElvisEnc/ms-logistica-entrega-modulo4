---
name: test-generator
description: >
  Genera unit tests de Domain y Application para un objetivo del MAPA de pruebas,
  siguiendo .claude/rules/unit-tests.md. Usar al escribir tests unitarios de un
  agregado, entidad hija, Value Object, command handler, política o traductor de
  eventos, o al corregir tests tras un informe de revisión.
---

# test-generator

Genérica: no nombra agregados, Value Objects, invariantes ni códigos de error de
ningún microservicio en particular. Todo dato específico se lee en tiempo de
ejecución de `docs/DESIGN.md` y `docs/testing/MAPA-UNIT-TESTS.md` del repositorio
donde se esté trabajando.

## Procedimiento

1. Recibe un objetivo tal como aparece en `docs/testing/MAPA-UNIT-TESTS.md` — o,
   si llega un informe de revisión, la lista de filas que señala.
2. Lee `.claude/rules/unit-tests.md` completo y localiza la fila o filas del
   objetivo en el MAPA.
3. Ubica el objetivo en el código real: `mcp__codegraph__codegraph_explore` si
   está disponible; si no, `Grep`/`Glob`.
4. Lee el código real y lo que le corresponde en `docs/DESIGN.md` §6
   (invariantes), §7 (máquinas de estado), §13 (catálogo de errores) y §19
   (desviaciones ya documentadas).
5. **Antes de escribir ningún test**, presenta una tabla: caso → regla `UT-xx` →
   código de invariante/error. Cubre camino feliz, cada rama de error, y caso
   negativo + positivo por invariante (UT-05, UT-06).
6. Escribe los tests en la ruta espejo de `src/` (UT-16) y crea o completa los
   builders/fakes que falten en `TestSupport/` (UT-12).
7. Corre `dotnet test` del proyecto afectado hasta que esté en verde.
8. Actualiza la columna «Estado» de la fila del MAPA (`Hecho`, o `INC-<n>` si
   aplica).
9. Devuelve la lista de archivos creados o modificados.

## Si el código de `src/` no cumple `DESIGN.md`

No se toca `src/`. El test se deja escrito con `[Fact(Skip = "INC-<n>")]`
(UT-15) y se aplica el protocolo de incoherencias de `CLAUDE.md`. Antes de
registrar la incoherencia, se revisa `docs/DESIGN.md` §19: si coincide con una
desviación `D-xx` ya documentada, la entrada la cita en vez de duplicarla. La
columna «Estado» de esa fila queda en `INC-<n>`.

## Si se recibe un informe `Veredicto: NO CUMPLE`

Se corrigen **solo** las filas que el informe señala (`Archivo:línea`, regla
`UT-xx`) y se vuelve a correr `dotnet test`. No se reescribe ningún test que el
informe no haya señalado.
