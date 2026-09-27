---
name: contract-tests
description: >
  Orquesta la generación y verificación de contract tests (Pact): delega en
  pact-writer y luego en pact-reviewer, con hasta 2 vueltas de corrección.
disable-model-invocation: true
argument-hint: "[par del MAPA | \"todo\"]"
---

# /contract-tests

Genérico: no nombra pares ni códigos de este microservicio. Este flujo lo ejecutas tú,
la sesión principal — nunca lo precargues en un agente ni lo dejes invocar solo.

Par recibido: `$ARGUMENTS`.

## Si `$ARGUMENTS` no es `"todo"`

1. Delega en el agente `pact-writer` el par `$ARGUMENTS`.
2. Delega en el agente `pact-reviewer` con la lista de archivos que devolvió
   `pact-writer`.
3. Si el informe dice `Veredicto: NO CUMPLE`: pasa el informe completo a `pact-writer` y
   repite el paso 2. Máximo 2 vueltas de corrección en total; si a la segunda sigue sin
   cumplir, te detienes y muestras el último informe tal cual, sin más intentos.
4. Si `Veredicto: CUMPLE`: resume: pactos generados o verificados, reglas `CT-xx`
   verificadas, y filas del MAPA completadas.

## Si `$ARGUMENTS` es `"todo"`

Recorre las filas pendientes de `docs/testing/MAPA-CONTRACT-TESTS.md`, en el orden de su
sección «Orden de ataque», repitiendo los pasos 1-4 de arriba por cada una. Un par cuya
columna «Transporte» diga "PENDIENTE" (esperando una incoherencia sin resolver) se anota
y no bloquea a los demás; se continúa con el resto y se informa al final qué filas
quedaron pendientes.
