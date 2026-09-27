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
