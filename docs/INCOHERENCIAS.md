# Incoherencias detectadas durante la construcción

> Protocolo en `CLAUDE.md`. **Claude Code no resuelve incoherencias: las reporta y para.**
> El estado solo lo cambio yo. El hook `SessionStart` cuenta las ABIERTAS al abrir sesión y
> `cierre-de-turno` lo recuerda al final de cada turno.

Las incoherencias del proyecto se cerraron antes de empezar: están en
`docs/DECISIONES.md`. Este archivo es para las que aparezcan
**escribiendo código**, y empieza vacío a propósito.

---

<!-- Formato de cada entrada:

### INC-1 · <título en una línea>
- **Detectada en:** <fase / archivo / momento>
- **Fuente A dice:** <cita textual + §sección>
- **Fuente B dice:** <cita textual + §sección>
- **Por qué no puedo continuar:** <qué decisión concreta bloquea>
- **Opciones:** A) … B) … C) …
- **Mi recomendación y por qué:** <una, con motivo técnico>
- **Impacto si se elige mal:** <qué se rompe después>
- **Estado:** ABIERTA

-->

### INC-1 · D-09 (evidencia): la capa que valida y el código de error no están definidos igual en dos fuentes
- **Detectada en:** Fase 4, plan de Infrastructure — `LocalEvidenciaStorage`
- **Fuente A dice:** `docs/DESIGN.md` §19.2, fila `D-09`, columna "Acción recomendada": *"Validar extensión y tipo contra una lista blanca de imágenes y aplicar un límite de tamaño **en el handler**. Una evidencia que no es una imagen no prueba nada ante un reclamo."* — es decir, en `ConfirmarEntregaCommandHandler` / `ReportarIncidenciaCommandHandler` (**Application**).
- **Fuente B dice:** `docs/GUIA.md`, Fase 4, bloque `[CC]`: *"`LocalEvidenciaStorage` resuelve la carpeta con `IHostEnvironment.ContentRootPath` y nombra el archivo `{paradaId}-{timestamp}{ext}`. **Valida extensión y tipo contra una lista blanca de imágenes y aplica un límite de tamaño**: la desviación D-09 de §19 es que aceptaba cualquier stream..."* — es decir, en `LocalEvidenciaStorage` (**Infrastructure**).
- **Por qué no puedo continuar:** no puedo decidir en qué capa vive el guard sin violar una de las dos fuentes, y **ninguna de las dos declara el código de error** que ese guard debe lanzar. El catálogo de `docs/DESIGN.md` §13 no tiene ninguna entrada para "extensión no permitida" ni "archivo excede el tamaño máximo", y `CLAUDE.md` prohíbe inventar códigos.
- **Opciones:**
  A) Validar en el handler de Application (`ConfirmarEntregaCommandHandler`/`ReportarIncidenciaCommandHandler`), antes de llamar a `IEvidenciaStorage.GuardarAsync` — sigue la letra de `DESIGN.md` §19, que manda sobre `GUIA.md` según la jerarquía de `CLAUDE.md`.
  B) Validar dentro de `LocalEvidenciaStorage.GuardarAsync` (Infrastructure) — sigue la letra de `GUIA.md`, y es coherente con que Infrastructure ya lanza `DomainException` desde los query handlers (`RUTA_NO_ENCONTRADA`, `RANGO_FECHAS_INVALIDO`).
  C) Duplicar el guard en ambas capas (defensa en profundidad) — resuelve el desacuerdo de ubicación pero no resuelve la falta de código de error, y es sobreingeniería para una sola validación.
- **Mi recomendación y por qué:** Opción A. `DESIGN.md` manda sobre `GUIA.md` (`CLAUDE.md`: "una guía no manda sobre DESIGN.md"), y Application ya es la capa que abre el stream (`ConfirmarEntregaCommandHandler` recibe `Stream? Archivo` y valida su obligatoriedad *antes* de tocar el repositorio, §10.1) — es natural que la misma comprobación de forma (extensión, tamaño) ocurra ahí, sin que `LocalEvidenciaStorage` necesite conocer reglas de negocio sobre qué es "una imagen válida". Propondría dos códigos nuevos siguiendo la nomenclatura de §13 (sin prefijo `Ixx`, como `EVIDENCIA_ARCHIVO_REQUERIDO`): `EVIDENCIA_EXTENSION_NO_PERMITIDA` y `EVIDENCIA_TAMANO_EXCEDIDO`, ambos `ErrorType.Validation` → 400, declarados en `RutaErrors` (mismo criterio que ya se usa para `EVIDENCIA_ARCHIVO_REQUERIDO`).
- **Impacto si se elige mal:** si el guard queda en Infrastructure, `revisor-ddd` puede marcarlo como una regla de negocio (qué cuenta como evidencia válida) filtrada hacia la capa equivocada; si el código de error no se fija en el catálogo antes de escribir el código, el test de contrato de esa validación documenta un código que oficialmente "no existe" en `DESIGN.md` §13.
- **Estado:** CERRADA

### INC-2 · PactNet 5.0.1 no puede verificar message pacts como proveedor: bug abierto upstream
- **Detectada en:** Tarea 3 (Contract testing), Fase 1 (entorno), al ejecutar el test de humo del lado proveedor (`PactVerifier(...).WithMessages(...).WithFileSource(...).Verify()`) contra un pacto generado por el propio proyecto en la misma sesión.
- **Fuente A dice:** `docs/DECISIONES.md`, INC-S39 (Resuelta): *"La Actividad 3 pide «al menos dos solicitudes realizadas desde el consumidor y verificadas desde el provider»... se usan message pacts sobre los eventos de §9"*, con `docs/DESIGN.md` §15.5 fijando: *"Proyecto `tests/Logistica.ContractTests`, con **PactNet 5** en modo *message pact*"*. Esta decisión asume que la verificación de proveedor de PactNet 5 en modo mensaje funciona.
- **Fuente B dice:** al ejecutar exactamente el patrón documentado por Pact (`https://docs.pact.io/implementation_guides/net/docs/messaging-pacts`, confirmado literal contra mi código), la verificación de proveedor falla siempre con `PactVerificationFailedException` → `"Request Failed - builder error for url (message://localhost:PUERTO/pact-messages)"`. Es un bug confirmado y **abierto** en el repositorio oficial: [pact-foundation/pact-net#558](https://github.com/pact-foundation/pact-net/issues/558), *"Provider verification with WithMessages fails with 'builder error for url (message://...)' due to incorrect URL scheme passed to FFI"* — afecta **5.0.0 y 5.0.1** (las dos únicas versiones publicadas de la línea 5.x; no hay una 5.0.2 con el fix), en Windows/macOS/Linux, tanto en spec V3 como V4. Sin workaround documentado en el issue (el propio issue lo confirma: "no comentarios resolutivos"). El lado **consumidor** (`WithMessageInteractions`, generación del pacto) no está afectado — lo verifiqué y funciona correctamente.
- **Por qué no puedo continuar:** el par #10 (BC6 proveedor de `ms-pacientes`) y el par #11 (BC6 proveedor de `ms-catering`) — dos de los tres roles del plan aprobado, incluido el elegido como mínimo viable de la actividad — dependen de que la verificación de proveedor de PactNet 5 funcione. Con este bug, **ningún** test de verificación de proveedor en modo mensaje puede pasar, sin importar cómo se escriba el código: el fallo ocurre en la FFI nativa antes de invocar ningún escenario. Seguir escribiendo los tests de los pares #10/#11 tal como están especificados produciría una suite que nunca puede estar en verde, contradiciendo el propio enunciado de la tarea ("verificada desde el provider").
- **Opciones:**
  A) Mantener PactNet 5 y message pacts para los tres pares, aceptar que la verificación de proveedor (#10, #11) no puede pasar hoy, implementar los tests igual (fieles al patrón documentado), marcarlos `[Fact(Skip = "...")]` citando el issue #558, y documentar extensamente que la actividad solo puede demostrar "provider verificado" de forma parcial (el lado consumidor sí, el proveedor no). Riesgo: no cumple la letra del enunciado ("al menos dos solicitudes... verificadas desde el provider").
  B) Para los pares donde BC6 es **proveedor** (#10, #11) — que son justo los que necesitan cumplir el requisito de verificación —, usar **Pact HTTP** (`WithHttpInteractions`/`PactVerifier.WithHttpEndpoint`, sin el bug: la ruta HTTP de PactNet 5 es la más madura y no aparece en ningún issue abierto equivalente) sobre el endpoint real `/api/integracion/*` que ya expone BC6 para simular la entrada de eventos (`docs/SISTEMA.md` §10.4 ya contempla esta simulación). Mantener message pacts para el par #9 (BC6 consumidor), que no está afectado por el bug. Esto es exactamente la alternativa que el propio INC-S39 dejó prevista: *"Si el docente exige HTTP, esta es la alternativa, y se implementa sobre el mismo par mínimo"* — aquí la razón no es una exigencia del docente sino un bug verificado, pero la alternativa técnica es la misma.
  C) Downgradear a PactNet 4.5.0 (última versión estable pre-5, con API de mensajería distinta, `IMessagePact`). Riesgo alto: no hay garantía de que el bug esté solo en el wrapper .NET y no en la librería nativa `pact_ffi` compartida entre versiones; además contradice literalmente "PactNet 5" de `DESIGN.md` §15.5 sin la certeza de que resuelva el problema, y la API de mensajería 4.x está marcada obsoleta desde antes de la migración a 5.0.
- **Mi recomendación y por qué:** Opción B. Es la única que permite cumplir el requisito literal de la actividad ("verificada desde el provider") con una ruta de PactNet 5 que sí funciona, preserva la mayor parte de la decisión original (message pacts se mantienen donde no hay bug, es decir en el rol de consumidor), y reutiliza infraestructura que el propio sistema ya documenta (`/api/integracion/*`, §10.4). Es reversible sin tocar `src/`: solo cambia cómo se escriben los tests de `Logistica.ContractTests` para los pares #10/#11.
- **Impacto si se elige mal:** si se sigue con la Opción A, la entrega no cumple el mínimo de la actividad y corre el riesgo señalado en el propio enunciado ("no habrá posibilidad de presentaciones posteriores"). Si se elige la Opción C sin verificar primero que el bug no está en la capa nativa compartida, se puede perder tiempo repitiendo el mismo fallo con una API ya obsoleta.
- **Estado:** CERRADA
