# Checklist de verificación — contract tests

Una fila por cada regla `CT-xx` de `.claude/rules/contract-tests.md`. La columna «Cómo se
comprueba» indica el método: **grep** (búsqueda de texto), **lectura** (inspección del test
contra la fuente citada) o **ejecución** (correr un comando).

| Regla | Qué exige (resumen) | Cómo se comprueba |
|---|---|---|
| CT-01 | Message pact de PactNet 5 por defecto, sin broker, `pacts/<Consumidor>-<Proveedor>.json` | Grep: `PactNet`, ausencia de configuración de Pact Broker; lectura del nombre de archivo generado |
| CT-02 | Pact HTTP solo si hay una incoherencia registrada que lo autoriza para ese par | Lectura: si el par usa HTTP, existe una referencia a la incoherencia (`INC-<n>`) que lo autorizó, citada en el test o en el MAPA |
| CT-03 | El shim HTTP invoca el traductor real, no lo reimplementa | Lectura: el shim publica el evento de dominio real vía el mecanismo de notificación real (MediatR u equivalente) y captura lo que el traductor real produjo; no construye a mano el JSON de respuesta |
| CT-04 | Lado consumidor: mensaje pasado por el handler/endpoint real | Lectura: el test invoca el handler/endpoint real (no solo afirma la forma del JSON); si sustituye un puerto de persistencia, es por un fake que captura, no un no-op silencioso |
| CT-05 | Lado proveedor: traductor real + `JsonSerializerOptions` reales | Lectura: las opciones de serialización citan o copian literalmente las del publicador real, con referencia a su archivo fuente |
| CT-06 | Campos opcionales en ambos estados (presente y null) | Lectura: cada campo opcional del evento tiene un caso con valor y uno con `null` |
| CT-07 | `[Trait("Capa", "Contrato")]` | Grep: el trait está presente en cada clase de test del archivo |
| CT-08 | Pactos "bootstrap" rotulados como transitorios | Grep/lectura: comentario explícito en el archivo que autora el pacto, y nota en el MAPA |
| CT-09 | Nunca se toca `src/` para que un test pase | Lectura: no hay cambios en `src/`; un test que revela un incumplimiento queda con `[Fact(Skip = "INC-<n>")]` y una incoherencia registrada o citada |
| CT-10 | Ubicación en espejo (`Consumidor/`, `Proveedor/`, `Testing/`) | Lectura: la clase vive en la carpeta que fija `contract-tests.md` según su rol |
| CT-11 | Solo `Assert` de xUnit | Grep: ausencia de `FluentAssertions` o cualquier otra librería de aserciones; solo `Assert.` |
| CT-12 | Qué no se prueba en este nivel | Lectura: no hay tests de bus de mensajería real, Pact Broker real, ni concurrencia entre publicaciones |
