# Checklist de verificación — unit tests

Una fila por cada regla `UT-xx` de `.claude/rules/unit-tests.md`. La columna
«Cómo se comprueba» indica el método: **grep** (búsqueda de texto), **lectura**
(inspección del test contra la fuente citada) o **ejecución** (correr un
comando).

| Regla | Qué exige (resumen) | Cómo se comprueba |
|---|---|---|
| UT-01 | Solo Domain/Application, con fakes; nada de infraestructura real | Lectura: sin `EF Core`, PostgreSQL, Testcontainers ni HTTP; los repositorios y publicadores usados son fakes en memoria |
| UT-02 | `[Trait("Capa", "Unit")]` en cada clase | Grep: `[Trait("Capa", "Unit")]` presente en cada clase de test del archivo |
| UT-03 | AAA, un comportamiento por test | Lectura: comentarios `// Arrange` `// Act` `// Assert` presentes y un solo comportamiento verificado por método |
| UT-04 | Nombre `Metodo_Escenario_Resultado` + código citado | Lectura y contraste contra `docs/DESIGN.md` §6 / `docs/SISTEMA.md`: el nombre sigue el patrón, y si prueba una invariante, el código aparece en el nombre y en un comentario `// Ixx (RN-xx)` |
| UT-05 | Caso negativo y positivo por invariante | Lectura: por cada invariante que le corresponde al objetivo, existe al menos un test que la viola y uno que la cumple |
| UT-06 | Camino feliz + cada rama de error, sin efectos en el fallo | Lectura: el handler tiene un test de camino feliz y uno por cada rama de error declarada; los de fallo afirman que no hubo cambios en el repositorio fake, ni commit del `UnitOfWork`, ni eventos publicados |
| UT-07 | `Error.Code` y `Error.Type` exactos | Lectura y contraste contra `docs/DESIGN.md` §13: el código y el `ErrorType` afirmados coinciden con el catálogo, no con uno parecido |
| UT-08 | Evento de dominio: tipo y payload completo | Lectura: se afirma el tipo concreto del evento y los campos de su payload, no solo que «se emitió algo» |
| UT-09 | Serialización de eventos de integración, con opcionales en `null` | Lectura y contraste contra `docs/SISTEMA.md` §9 / la sección equivalente de `DESIGN.md`: el test compara todos los campos del contrato, incluidos los opcionales presentes con valor `null`, sin aplanar objetos anidados |
| UT-10 | Idempotencia de eventos entrantes | Lectura: el test procesa el mismo evento dos veces y afirma que el efecto ocurrió una sola vez |
| UT-11 | Determinismo | Grep: ausencia de `DateTime.Now`/`DateTime.UtcNow` como fuente de la fecha bajo prueba, `Guid` aleatorio en una aserción, `Thread.Sleep` y estado estático mutable; lectura: las fechas de arrange/assert son fijas, en UTC, con sufijo `Z` |
| UT-12 | Datos de prueba en `TestSupport/`, sin lógica condicional en el test | Lectura: los builders/fakes usados viven en `TestSupport/` del proyecto; el cuerpo del test no contiene `if`, `for` ni `while` |
| UT-13 | Qué no se prueba en este nivel | Lectura: no hay tests de getters triviales, de miembros privados por reflexión, ni del comportamiento de `Joseco.DDD.Core`, MediatR o EF Core |
| UT-14 | Solo `Assert` de xUnit | Grep: ausencia de `FluentAssertions` o cualquier otra librería de aserciones; solo `Assert.` |
| UT-15 | Nunca se toca `src/` para que un test pase | Lectura: no hay cambios en `src/`; un test que revela un incumplimiento queda con `[Fact(Skip = "INC-<n>")]` y una incoherencia registrada o citada |
| UT-16 | Ubicación en espejo de `src/` | Lectura: la ruta del archivo de test, dentro de `tests/<Proyecto>.Tests/`, replica la carpeta relativa del tipo bajo prueba en `src/<Proyecto>/` |
| UT-17 | Cobertura ≥ 80 % de líneas sobre Domain + Application | Ejecución: `dotnet test` en verde y `scripts/test-cobertura.ps1` reporta ≥ 80 % de líneas sobre los ensamblados `Domain` y `Application` |
