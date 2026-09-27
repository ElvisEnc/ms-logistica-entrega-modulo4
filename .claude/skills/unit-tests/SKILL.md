---
name: unit-tests
description: >
  Orquesta la generación y verificación de unit tests: delega en test-writer y luego
  en test-reviewer, con hasta 2 vueltas de corrección.
disable-model-invocation: true
argument-hint: "[objetivo del MAPA | \"todo\"]"
---

# /unit-tests

Genérico: no nombra agregados ni códigos de este microservicio. Este flujo lo
ejecutas tú, la sesión principal — nunca lo precargues en un agente ni lo dejes
invocar solo.

Objetivo recibido: `$ARGUMENTS`.

## Si `$ARGUMENTS` no es `"todo"`

1. Delega en el agente `test-writer` el objetivo `$ARGUMENTS`.
2. Delega en el agente `test-reviewer` con la lista de archivos que devolvió
   `test-writer`.
3. Si el informe dice `Veredicto: NO CUMPLE`: pasa el informe completo a
   `test-writer` y repite el paso 2. Máximo 2 vueltas de corrección en total; si
   a la segunda sigue sin cumplir, te detienes y muestras el último informe tal
   cual, sin más intentos.
4. Si `Veredicto: CUMPLE`: ejecuta `scripts/test-cobertura.ps1` y resume: tests
   nuevos, reglas `UT-xx` verificadas, % de líneas y de ramas, y filas del MAPA
   completadas.

## Si `$ARGUMENTS` es `"todo"`

Recorre las filas pendientes de `docs/testing/MAPA-UNIT-TESTS.md`, en el orden
de su sección «Orden de ataque», repitiendo los pasos 1-4 de arriba por cada
una. Una fila que sigue atascada tras las 2 vueltas se anota con el motivo y no
bloquea a las siguientes; se continúa con el resto y se informa al final qué
filas quedaron pendientes.
