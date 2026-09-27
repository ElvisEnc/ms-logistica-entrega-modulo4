---
name: contract-test-generator
description: >
  Genera contract tests (Pact) de un par del MAPA de contract tests, siguiendo
  .claude/rules/contract-tests.md. Usar al escribir la clase de test de un par
  (message pact o, bajo CT-02, Pact HTTP sobre un shim de solo test), o al
  corregir tests tras un informe de revisión.
---

# contract-test-generator

Genérica: no nombra pares, eventos ni códigos de error de ningún microservicio en
particular. Todo dato específico se lee en tiempo de ejecución de `docs/SISTEMA.md`,
`docs/DESIGN.md` y `docs/testing/MAPA-CONTRACT-TESTS.md` del repositorio donde se esté
trabajando.

## Procedimiento

1. Recibe un par tal como aparece en `docs/testing/MAPA-CONTRACT-TESTS.md` — o, si
   llega un informe de revisión, la lista de filas que señala.
2. Lee `.claude/rules/contract-tests.md` completo y localiza la fila o filas del par en
   el MAPA, incluida su columna «Transporte».
3. Si la columna «Transporte» dice "PENDIENTE" (esperando una incoherencia sin
   resolver): para y dilo explícitamente, sin escribir código para ese par. Los demás
   pares no bloqueados sí se generan.
4. Ubica el traductor/handler/endpoint real del par con
   `mcp__codegraph__codegraph_explore` si está disponible; si no, `Grep`/`Glob`.
5. Lee el código real y lo que le corresponde en `docs/SISTEMA.md` §9 (forma exacta del
   evento, campos opcionales incluidos) y §13.5 (a qué par pertenece).
6. **Antes de escribir ningún test**, presenta una tabla: interacción → regla `CT-xx` →
   campos con matcher → caso opcional cubierto.
7. Escribe la clase de test en la ubicación que declare `.claude/rules/contract-tests.md`
   (CT-10):
   - Si el transporte es *message pact*: consumidor real con
     `Pact.V4(...).WithMessageInteractions()`, pasando el mensaje por el handler/endpoint
     real (CT-04).
   - Si el transporte es *Pact HTTP sobre shim* (CT-02): reutiliza o completa el shim de
     `tests/<Proyecto>.ContractTests/Testing/` (CT-03) y escribe tanto el lado que autora
     el pacto (rotulado como bootstrap si aplica, CT-08) como el lado que lo verifica
     contra el shim real.
8. Corre `dotnet test --filter Capa=Contrato` hasta que el par esté en verde.
9. Actualiza la columna «Estado» de la fila o filas del MAPA (`Hecho`, o `INC-<n>` si
   aplica).
10. Devuelve la lista de archivos creados o modificados.

## Si el código de `src/` no cumple `SISTEMA.md`/`DESIGN.md`

No se toca `src/`. El test se deja escrito con `[Fact(Skip = "INC-<n>")]` (CT-09) y se
aplica el protocolo de incoherencias de `CLAUDE.md`.

## Si se recibe un informe `Veredicto: NO CUMPLE`

Se corrigen **solo** las filas que el informe señala (`Archivo:línea`, regla `CT-xx`) y
se vuelve a correr `dotnet test`. No se reescribe ningún test que el informe no haya
señalado.
