# Checklist de verificación — integration tests

Una fila por cada regla `IT-xx` de `.claude/rules/integration-tests.md`. La columna
«Cómo se comprueba» indica el método: **grep** (búsqueda de texto), **lectura**
(inspección del test contra la fuente citada) o **ejecución** (correr un
comando).

| Regla | Qué exige (resumen) | Cómo se comprueba |
|---|---|---|
| IT-01 | `WebApplicationFactory<Program>` + `Testcontainers.PostgreSql` real; nunca en memoria ni contra `nur-tricenter-postgres` | Lectura: sin `UseInMemoryDatabase` ni repositorios fake; grep: `Testcontainers.PostgreSql`, imagen `postgres:16`, ausencia de `nur-tricenter-postgres` |
| IT-02 | Un contenedor por colección, migraciones al arrancar, base limpia por clase | Lectura: la fixture aplica `Database.MigrateAsync()` explícitamente y no depende de `IsDevelopment()`; los tests no comparten estado entre sí |
| IT-03 | `[Trait("Capa", "Integracion")]` + `[Trait("Flujo", "F<n>-<nombre>")]` | Grep: ambos traits presentes en cada clase de test del archivo, con el nombre de flujo que declara el MAPA |
| IT-04 | Frontera: publicador que captura, eventos entrantes por HTTP, nunca otro microservicio real | Lectura: `IIntegrationEventPublisher` (o el puerto equivalente) está sustituido por una implementación que captura; ninguna llamada sale a otro microservicio |
| IT-05 | Camino correcto y camino incorrecto por flujo | Lectura: el flujo tiene al menos un test de camino correcto y uno de camino incorrecto, según las filas del MAPA |
| IT-06 | HTTP y `codigo` exactos, citados del flujo real | Lectura y contraste contra `docs/DESIGN.md` §11/§12/§13: el HTTP y el `codigo` afirmados coinciden con la fila del MAPA, no con un camino parecido de otro flujo |
| IT-07 | Eventos de integración publicados: tipo y payload completo | Lectura: se afirma el tipo concreto del evento capturado y los campos de su payload, no solo que "se publicó algo" |
| IT-08 | Determinismo | Grep: ausencia de `DateTime.Now`/`DateTime.UtcNow` como fuente de la fecha bajo prueba, `Guid` aleatorio en una aserción, `Thread.Sleep` y estado estático mutable |
| IT-09 | Nunca se toca `src/` para que un test pase | Lectura: no hay cambios en `src/`; un test que revela un incumplimiento queda con `[Fact(Skip = "INC-<n>")]` y una incoherencia registrada o citada |
| IT-10 | Una clase de test por flujo, en la carpeta acordada | Lectura: la clase vive en la ubicación que fija `integration-tests.md`, y no mezcla dos flujos en la misma clase |
| IT-11 | Solo `Assert` de xUnit | Grep: ausencia de `FluentAssertions` o cualquier otra librería de aserciones; solo `Assert.` |
| IT-12 | Qué no se prueba en este nivel | Lectura: no hay tests de concurrencia, de e2e con otro microservicio real, ni de lo que `docs/DESIGN.md` §15.7 registre como riesgo aceptado |
