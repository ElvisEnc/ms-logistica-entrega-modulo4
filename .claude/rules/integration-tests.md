---
paths:
  - "tests/**/*IntegrationTests*/**/*.cs"
---

# Reglas de integration tests (módulo 4)

Genéricas para los seis microservicios de NUR-TRICENTER. No nombran agregados, flujos,
Value Objects ni códigos de error de ningún microservicio en particular: esos datos se
leen en tiempo de ejecución del `docs/DESIGN.md` del repositorio donde se esté
trabajando. Cada regla tiene un ID estable (`IT-xx`) que no se renumera.

## IT-01 · Alcance: `WebApplicationFactory<Program>` + Testcontainers PostgreSQL

Integration tests solo con `Microsoft.AspNetCore.Mvc.Testing`
(`WebApplicationFactory<Program>`) y `Testcontainers.PostgreSql` (imagen `postgres:16`).
El contenedor lo levanta el propio test; nunca se usa un PostgreSQL local ya corriendo
(por ejemplo, el contenedor de desarrollo del `CLAUDE.md`). Prohibido sustituir la base
por un proveedor en memoria (`UseInMemoryDatabase`) o por repositorios fake: eso es
unit test, un nivel distinto.

**Por qué:** `SISTEMA.md` §13.4 — «el contenedor lo levanta el test; no se usa
[el PostgreSQL de desarrollo]»; es el nivel que cierra el hueco de mapeos EF Core,
índices, binding y traducción `ErrorType` → HTTP que un unit test con fakes no cubre.

## IT-02 · Aislamiento: un contenedor por colección, base limpia por clase

Un contenedor Postgres por colección de xUnit (`ICollectionFixture`), con las
migraciones aplicadas al arrancar. Cada clase de test parte de una base limpia. Ningún
test depende del orden de ejecución ni del resultado de otro test.

**Por qué:** `SISTEMA.md` §13.4 — «un contenedor por colección de xUnit, migraciones
aplicadas al arrancar y base limpia por clase de test; los tests no dependen del
orden».

## IT-03 · `[Trait("Capa", "Integracion")]` + `[Trait("Flujo", "F<n>-<nombre>")]`

Toda clase de test de este nivel lleva `[Trait("Capa", "Integracion")]`. Además, lleva
`[Trait("Flujo", "F<n>-<nombre>")]` según el flujo que cubra, con el número y el nombre
tal como los fija la tabla de flujos de `docs/DESIGN.md` §15.4 (o la sección
equivalente que ese documento use para enumerar los flujos de integración).

**Por qué:** son los traits que permiten `dotnet test --filter Capa=Integracion` y
distinguir un flujo de otro (`CLAUDE.md`, «Pruebas (módulo 4)»; `SISTEMA.md` §13.4 —
«cada flujo del §12 del DESIGN.md es una clase de test con
`[Trait("Flujo", "F<n>-<nombre>")]`»).

## IT-04 · Frontera: publicador de eventos capturado, entrantes por HTTP simulado

En la `WebApplicationFactory`, el publicador de eventos de integración se sustituye por
uno que **captura** los eventos publicados en memoria, para poder afirmar qué se
publicó y con qué payload. Los eventos de integración entrantes se simulan llamando al
endpoint HTTP de integración del microservicio (`POST /api/integracion/*` o el que
`docs/DESIGN.md` documente). Nunca se llama a otro microservicio real ni se levanta más
de un microservicio en el mismo test.

**Por qué:** `SISTEMA.md` §13.4 — «el `IIntegrationEventPublisher` se sustituye en la
factory por un publicador que captura los eventos [...]. Los eventos entrantes se
simulan llamando a `POST /api/integracion/*`. No se llama a otros microservicios».

## IT-05 · Por flujo: camino correcto y camino incorrecto

Cada flujo cubierto tiene al menos un test de camino correcto y uno de camino
incorrecto. El camino incorrecto afirma: el código HTTP exacto, el `codigo` exacto del
cuerpo `{codigo, mensaje}` (nunca solo el HTTP), y que el estado persistido no cambió
—releyendo el agregado o la fila desde la base, no desde una variable en memoria del
test.

**Por qué:** `SISTEMA.md` §13.4 — «mínimo por microservicio: dos flujos completos, y en
ellos al menos un camino correcto y uno incorrecto. El incorrecto afirma el HTTP y el
`codigo` del cuerpo, y que el estado persistido no cambió».

## IT-06 · Códigos de error y HTTP citados del catálogo, no inventados

El `codigo` y el HTTP que afirma un camino incorrecto se citan del catálogo de
`docs/DESIGN.md` §13 y de los flujos de `docs/DESIGN.md` §12 (o las secciones
equivalentes que ese documento use para el catálogo de errores y los flujos de caso de
uso). Nunca se copian de un caso parecido ni se inventan.

**Por qué:** mismo principio que `CLAUDE.md` fija para el código de producción
—«nunca se copia el `ErrorType` de un código parecido»— aplicado a la aserción del
test: un código de error mal citado hace que el test pase verificando la regla
equivocada.

## IT-07 · Eventos de integración publicados: tipo y payload completo

Un test que verifica un evento de integración publicado afirma su tipo concreto y el
payload completo capturado por el publicador fake —incluidos los campos opcionales
presentes con valor `null` y sin aplanar los objetos anidados del contrato—, igual que
en el nivel unit (reglas equivalentes a UT-08/UT-09 de `.claude/rules/unit-tests.md`).
Nunca se afirma solo que «se publicó algo».

**Por qué:** `SISTEMA.md` §13.3 exige comparar exactamente contra la tabla del evento
en §9 del maestro; ese mismo estándar de exhaustividad aplica cuando el evento se
observa a través del publicador capturado en un test de integración.

## IT-08 · Determinismo

Prohibido dentro del cuerpo de un test: `DateTime.Now`/`DateTime.UtcNow` como fuente de
la fecha bajo prueba, `Guid` aleatorio en una aserción, `Thread.Sleep`, dependencia del
orden de ejecución entre tests, y estado estático mutable compartido entre tests. Las
fechas usadas en arrange y assert son fijas, en UTC, con sufijo `Z`.

**Por qué:** mismo motivo que UT-11 de `.claude/rules/unit-tests.md` — comparar contra
el reloj de la máquina en vez de una fecha fija produce un test no determinista que
falla de forma intermitente, y en integración el efecto se agrava porque el contenedor
puede tardar en arrancar.

## IT-09 · Nunca se toca `src/` para que un test pase

Un test de este nivel nunca se hace pasar modificando código de `src/`. Si un test
revela que el código no cumple `docs/DESIGN.md`, se deja escrito con
`[Fact(Skip = "INC-<n>")]` y se aplica el protocolo de incoherencias de `CLAUDE.md`.
Antes de registrar la incoherencia, se revisa `docs/DESIGN.md` §19: si coincide con una
desviación `D-xx` ya documentada, la entrada la cita en vez de duplicarla.

**Por qué:** `SISTEMA.md` §13.7 — «un test nunca se hace pasar tocando `src/`. Si
revela una contradicción con `DESIGN.md`, se aplica el protocolo de incoherencias»
(idéntico a UT-15, aplicado aquí al nivel de integración).

## IT-10 · Ubicación en espejo por flujo

Una clase de test por flujo en `tests/<Proyecto>.IntegrationTests/Flujos/<Flujo>Tests.cs`,
donde `<Flujo>` es el nombre corto del flujo tal como lo nombra su trait
(`F<n>-<nombre>` sin el prefijo `F<n>-`). Si el proyecto de integration tests usa otra
carpeta raíz equivalente a `Flujos/`, se documenta aquí mismo antes de usarse.

**Por qué:** mismo principio que UT-16 de `.claude/rules/unit-tests.md` — localizar sin
buscar dónde vive el test de un flujo, esta vez organizado por flujo de caso de uso en
lugar de por tipo de `src/`, que es como los agrupa `docs/DESIGN.md` §15.4.

## IT-11 · Aserciones solo con `Assert` de xUnit

Ninguna aserción usa FluentAssertions ni ninguna otra librería de aserciones. Solo
`Assert` de xUnit.

**Por qué:** `CLAUDE.md`, «Pruebas (módulo 4)» — «Aserciones con `Assert` de xUnit.
Nada de FluentAssertions (INC-S38)»: desde su versión 8, FluentAssertions es de
licencia comercial.

## IT-12 · Qué no se prueba en este nivel

No se escriben tests de integración para: concurrencia (condiciones de carrera entre
requests simultáneos), pruebas end-to-end contra otros microservicios reales, ni
validación de tamaño o tipo de archivo de una evidencia subida. Eso queda fuera de
alcance del módulo 4 según `docs/DESIGN.md` §15.7 (o la sección equivalente que ese
documento use para delimitar lo que el módulo 4 no cubre).

**Por qué:** `docs/DESIGN.md` §15.7 fija explícitamente ese límite; probarlo aquí
duplicaría esfuerzo sobre un alcance que el propio plan de pruebas excluye.
