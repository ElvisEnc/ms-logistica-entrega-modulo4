---
paths:
  - "tests/**/*.cs"
---

# Reglas de unit tests (módulo 4)

Genéricas para los seis microservicios de NUR-TRICENTER. No nombran agregados, Value
Objects, invariantes ni códigos de error de ningún microservicio en particular: esos
datos se leen en tiempo de ejecución del `docs/DESIGN.md` del repositorio donde se
esté trabajando. Cada regla tiene un ID estable (`UT-xx`) que no se renumera.

## UT-01 · Alcance: Domain + Application, sin infraestructura

Unit tests solo sobre los agregados, entidades hijas y Value Objects de `Domain`, y
sobre los command handlers, políticas y traductores de eventos de `Application` con
repositorios y publicadores **fake en memoria**. Nada de EF Core, PostgreSQL,
Testcontainers ni HTTP en este nivel.

**Por qué:** `SISTEMA.md` §13.1 (Domain — obligatorio) y §13.2 (Application — con
fakes) delimitan exactamente estos dos proyectos; la integración contra
infraestructura real es §13.4, un nivel distinto.

## UT-02 · `[Trait("Capa", "Unit")]` en cada clase

Toda clase de test de este nivel lleva `[Trait("Capa", "Unit")]`.

**Por qué:** es el trait que permite `dotnet test --filter Capa=Unit`, tal como lo
fija `CLAUDE.md` en «Pruebas (módulo 4)».

## UT-03 · AAA, un comportamiento por test

Cada método de test cubre un solo comportamiento y marca sus tres bloques con
comentarios `// Arrange`, `// Act`, `// Assert`.

**Por qué:** `SISTEMA.md` §13.1 exige que un test sea trazable a una invariante o a
una rama concreta; mezclar comportamientos en un mismo método rompe esa trazabilidad
cuando el test falla.

## UT-04 · Nombre `Metodo_Escenario_Resultado` y código citado

El nombre del método sigue el patrón `Metodo_Escenario_Resultado`. Si el test
verifica una invariante, el código va en el nombre y se repite en un comentario con
el formato de `CLAUDE.md`, por ejemplo `// I4 (RN-16)`. Los códigos de invariante
(`Ixx`) y de regla de negocio (`RN-xx`) se copian literalmente de `docs/DESIGN.md`
§6 y de `docs/SISTEMA.md`; nunca se inventan ni se renumeran.

**Por qué:** `SISTEMA.md` §13.1 pide que «el nombre del test cita el código de la
invariante, de modo que una invariante sin test es visible por ausencia», y
`CLAUDE.md` fija el formato del comentario en sus «Reglas no negociables».

## UT-05 · Caso negativo y positivo por invariante

Por cada invariante que `docs/DESIGN.md` §6 sitúa en el objetivo bajo prueba: como
mínimo un test que la viole y uno que la cumpla. Los valores límite se cubren con
`[Theory]`/`[InlineData]`, no con varios `[Fact]` casi idénticos.

**Por qué:** `SISTEMA.md` §13.1 — «como mínimo un test por invariante declarada, con
su caso negativo y positivo».

## UT-06 · Command handlers: camino feliz + cada rama de error

Cada command handler tiene un test de camino feliz y un test por cada rama de error
que declara. En los tests de fallo se afirma además que **no hubo efectos**:
repositorio fake sin cambios, `UnitOfWork` sin commit, publicador sin eventos.

**Por qué:** `SISTEMA.md` §13.2 — «el camino feliz y cada rama de error de cada
handler, afirmando en los fallos que no hubo efectos».

## UT-07 · `Error.Code` y `Error.Type` exactos

Cada aserción de error afirma `Error.Code` **y** `Error.Type` (`ErrorType`) tal como
los declara el catálogo de `docs/DESIGN.md` §13. Prohibido copiar el `ErrorType` de
un código parecido: se busca el código exacto en el catálogo antes de afirmarlo.

**Por qué:** `SISTEMA.md` §13.1 pide afirmar «`Error.Code` **y** `Error.Type`,
exactamente como los declara el §13 del `DESIGN.md`»; `CLAUDE.md` insiste en que
«nunca se copia el `ErrorType` de un código parecido».

## UT-08 · Eventos de dominio: tipo y payload completo

Un test que verifica un evento de dominio afirma su tipo concreto y los campos de su
payload, no solo que «se emitió algo».

**Por qué:** el evento de dominio es el resultado observable de una operación del
agregado bajo prueba (`SISTEMA.md` §13.1); afirmar solo su existencia no prueba la
regla de negocio que lo generó.

## UT-09 · Serialización de eventos de integración

Por cada evento de integración que el repositorio publica, un test liviano de
serialización que compare los campos contra la tabla del evento en `docs/SISTEMA.md`
§9 (o la sección equivalente de `docs/DESIGN.md` que la detalla para este
microservicio), **incluidos los campos opcionales presentes con valor `null`**. Los
objetos anidados del contrato no se aplanan.

**Por qué:** `SISTEMA.md` §13.3 — «un test liviano de serialización que verifique
que el publicador serializa y el consumidor deserializa exactamente los campos
documentados, incluidos los opcionales».

## UT-10 · Idempotencia de eventos entrantes

El handler de un evento de integración entrante tiene un test que lo procesa dos
veces con el mismo payload y afirma que el efecto ocurrió **una sola vez**.

**Por qué:** `SISTEMA.md` §13.2 — «idempotencia de los endpoints de integración:
reprocesar el mismo evento no duplica el efecto».

## UT-11 · Determinismo

Prohibido dentro del cuerpo de un test: `DateTime.Now`/`DateTime.UtcNow` como fuente
de la fecha bajo prueba, `Guid` aleatorio en una aserción, `Thread.Sleep`, dependencia
del orden de ejecución entre tests, y estado estático mutable compartido entre tests.
Las fechas usadas en arrange y assert son fijas, en UTC, con sufijo `Z`.

**Por qué:** `Joseco.DDD.Core` construye `DomainEvent.OccuredOn` con `DateTime.Now`
(hora local) y fija los eventos de integración en `DateTime.UtcNow` al construirse
(`CLAUDE.md`, sección «Joseco.DDD.Core»); un test que compara contra el reloj de la
máquina en vez de una fecha fija es no determinista y falla de forma intermitente.

## UT-12 · Datos de prueba en `TestSupport/`

Los builders y fakes que necesite la suite viven en una carpeta `TestSupport/` del
proyecto de test correspondiente; se crean si no existen. El cuerpo de un test no
contiene `if`, `for` ni `while`: la variación de casos se expresa con
`[Theory]`/`[InlineData]` o con builders, no con lógica condicional dentro del test.

**Por qué:** sostiene UT-03 (un comportamiento, legible de un vistazo) y evita que la
propia suite necesite pruebas para confiar en su lógica.

## UT-13 · Qué no se prueba en este nivel

No se escriben tests de unit para: getters triviales, miembros privados accedidos
por reflexión, ni el comportamiento del framework (`Joseco.DDD.Core`, MediatR, EF
Core). Un test de este nivel prueba una regla de negocio propia del repositorio, un
contrato de evento o un caso de idempotencia.

**Por qué:** `SISTEMA.md` §13 (introducción) — «se prueba lo que protege una regla
de negocio, un contrato o un caso de idempotencia»; lo demás no es responsabilidad de
este nivel.

## UT-14 · Aserciones solo con `Assert` de xUnit

Ninguna aserción usa FluentAssertions ni ninguna otra librería de aserciones. Solo
`Assert` de xUnit.

**Por qué:** `CLAUDE.md`, «Pruebas (módulo 4)» — «Aserciones con `Assert` de xUnit.
Nada de FluentAssertions (INC-S38)»: desde su versión 8, FluentAssertions es de
licencia comercial.

## UT-15 · Nunca se toca `src/` para que un test pase

Un test nunca se hace pasar modificando código de `src/`. Si un test revela que el
código no cumple `docs/DESIGN.md`, se deja escrito con `[Fact(Skip = "INC-<n>")]` y
se aplica el protocolo de incoherencias de `CLAUDE.md`. Antes de registrar la
incoherencia, se revisa `docs/DESIGN.md` §19: si coincide con una desviación `D-xx`
ya documentada, la entrada la cita en vez de duplicarla.

**Por qué:** `SISTEMA.md` §13.7 — «un test nunca se hace pasar tocando `src/`. Si
revela una contradicción con `DESIGN.md`, se aplica el protocolo de incoherencias».

## UT-16 · Ubicación en espejo de `src/`

Cada tipo bajo prueba en `src/<Proyecto>/<Carpeta>/Tipo.cs` tiene su test en
`tests/<Proyecto>.Tests/<Carpeta>/TipoTests.cs`, con la misma carpeta relativa.

**Por qué:** mantiene localizable, sin buscar, dónde vive el test de cualquier tipo
de `src/`, siguiendo la organización por carpeta de caso de uso que fija `CLAUDE.md`
en su sección «Estructura».

## UT-17 · Cobertura ≥ 80 % de líneas sobre Domain + Application

La suite de este nivel, medida con `scripts/test-cobertura.ps1` y
`coverage.runsettings`, alcanza al menos 80 % de cobertura de líneas sobre los
ensamblados `Domain` y `Application` del repositorio.

**Por qué:** `SISTEMA.md` §13.6 — «reporte oficial de unit tests: ≥ 80 % de líneas
sobre `Domain` + `Application`» (INC-S40).
