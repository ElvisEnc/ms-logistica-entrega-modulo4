---
name: integration-tests
description: >
  Orquesta la generación y verificación de integration tests: delega en
  integration-test-writer y luego en integration-test-reviewer, con hasta 2
  vueltas de corrección.
disable-model-invocation: true
argument-hint: "[flujo del MAPA | \"todo\"]"
---

# /integration-tests

Genérico: no nombra flujos ni códigos de este microservicio. Este flujo lo
ejecutas tú, la sesión principal — nunca lo precargues en un agente ni lo
dejes invocar solo.

Flujo recibido: `$ARGUMENTS`.

## Si `$ARGUMENTS` no es `"todo"`

1. Delega en el agente `integration-test-writer` el flujo `$ARGUMENTS`.
2. Delega en el agente `integration-test-reviewer` con la lista de archivos
   que devolvió `integration-test-writer`.
3. Si el informe dice `Veredicto: NO CUMPLE`: pasa el informe completo a
   `integration-test-writer` y repite el paso 2. Máximo 2 vueltas de
   corrección en total; si a la segunda sigue sin cumplir, te detienes y
   muestras el último informe tal cual, sin más intentos.
4. Si `Veredicto: CUMPLE`: ejecuta `scripts/test-cobertura.ps1` (reporte
   combinado unit + integración, informativo) y resume: tests nuevos, reglas
   `IT-xx` verificadas, y filas del MAPA completadas.

## Si `$ARGUMENTS` es `"todo"`

Recorre las filas pendientes de `docs/testing/MAPA-INTEGRATION-TESTS.md`, en
el orden de su sección «Orden de ataque», repitiendo los pasos 1-4 de arriba
por cada una. Un flujo que sigue atascado tras las 2 vueltas se anota con el
motivo y no bloquea a los siguientes; se continúa con el resto y se informa al
final qué filas quedaron pendientes.
