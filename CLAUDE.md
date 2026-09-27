# ms-logistica-entrega — Guía de trabajo

Microservicio del sistema NUR-TRICENTER. BC6 — Logística de Entrega. Subdominio
genérico. La especificación vinculante son `docs/SISTEMA.md` (copia literal del
maestro, no se edita aquí) y `docs/DESIGN.md`. Ambos mandan sobre el código.

## Jerarquía documental

Todo lo que necesitas está bajo `docs/`, en este repositorio. **No hay nada que buscar
fuera de él:** la guía, la especificación y el maestro se copiaron aquí al montarlo.

| Archivo | Qué es |
|---|---|
| `docs/SISTEMA.md` | El maestro del sistema. Copia literal, no se edita aquí |
| `docs/DESIGN.md` | La especificación de este microservicio |
| `docs/GUIA.md` | El procedimiento de las seis fases, con el prompt y la condición de cierre de cada una |
| `docs/DECISIONES.md` | Las decisiones de proyecto ya cerradas, con lo que se descartó y por qué |
| `docs/JOSECO-DDD-CORE.md` | La API real del núcleo DDD y sus trampas |
| `docs/INCOHERENCIAS.md` | Las contradicciones detectadas aquí. Empieza vacío |

SISTEMA.md manda sobre DESIGN.md, y DESIGN.md manda sobre el código.
Si algo debe cambiar en un contrato de integración o en una convención
común, se cambia en SISTEMA.md primero. Nunca al revés.

**Las guías dicen cómo se construye; la especificación dice qué se construye.** Una guía
no manda sobre `DESIGN.md`: si la contradice, es una incoherencia y se registra.

## Protocolo de incoherencias — PARADA OBLIGATORIA

Si detectas una contradicción entre docs/SISTEMA.md, docs/DESIGN.md y/o el código:

1. NO la resuelvas por tu cuenta. NO elijas la opción que te parezca mejor.
2. NO sigas escribiendo código que dependa del punto en disputa.
3. Añade una entrada a `docs/INCOHERENCIAS.md` con este formato exacto:

   ### INC-<n> · <título en una línea>
   - **Detectada en:** <fase / archivo / momento>
   - **Fuente A dice:** <cita textual + §sección>
   - **Fuente B dice:** <cita textual + §sección>
   - **Por qué no puedo continuar:** <qué decisión concreta bloquea>
   - **Opciones:** A) … B) … C) …
   - **Mi recomendación y por qué:** <una, con motivo técnico>
   - **Impacto si se elige mal:** <qué se rompe después>
   - **Estado:** ABIERTA

4. Dímelo en la respuesta, en la primera línea, empezando por `⚠ INC-<n>`.
5. Si puedes seguir con otra parte que no dependa de eso, sigue y déjalo
   aislado. Si no, para y espera.

Nunca cambies el estado de una incoherencia. Eso lo hago yo.

## Reglas no negociables

- **Modelo rico.** Ningún setter público en el dominio. Nada de clases con solo
  propiedades y la lógica en el handler: eso es el modelo anémico y es lo que
  la Actividad 3 penaliza.
- **Las invariantes viven donde dice §6 de docs/DESIGN.md**, que para cada una
  indica si es del agregado, del VO, de la entidad hija o del handler. No se
  mueven de sitio. Cada comprobación cita su código: `// I4 (RN-16)`.
- **Los códigos no se inventan ni se renumeran.** RN-xx y HU-xx salen de
  SISTEMA.md; I1–I14 de docs/DESIGN.md §6. Si falta uno, es una incoherencia.
- **Regla de dependencias:** Domain ← Application ← Infrastructure ← WebApi.
  `Domain` referencia únicamente `Joseco.DDD.Core`; nada de EF Core, HTTP ni
  MediatR, salvo los `record : DomainEvent`.
- **Errores.** Los agregados lanzan `DomainException(Error)` con un `Error` del
  catálogo de docs/DESIGN.md §13 y el `ErrorType` exacto que ahí se declara
  (SISTEMA.md §12.1). Nunca excepciones nativas de .NET desde Domain, nunca un
  código inventado, y **nunca se copia el ErrorType de un código parecido**.
  Desde `Domain` solo salen `Validation` y `Conflict`.
- **Nunca `Parse`, siempre `TryParse`** (SISTEMA.md §12.1.1). Ninguna
  conversión de texto a enum, número, `Guid` o fecha —binding de un DTO,
  mapeo de un evento entrante, lectura de configuración— puede lanzar una
  excepción nativa. `Enum.Parse<T>(...)` sin guarda devuelve **500** donde
  debía devolver 400. La conversión fallida lanza `DomainException` con
  código de catálogo.
- **Colecciones**: campo privado `_paradas` + `IReadOnlyCollection<T>` público.
- **Idempotencia** en el único endpoint de integración entrante (I12).
- Un caso de uso, una carpeta, con su Command y su Handler.

## Joseco.DDD.Core

La API real está en `docs/JOSECO-DDD-CORE.md`. Léela antes de usar cualquier
tipo del paquete. En particular:

- NO existe `Error.Validation(...)`. Se usa el constructor:
  `new Error(codigo, mensaje, ErrorType.Validation, args)`.
- `DomainEvent.OccuredOn` va con una sola `r` y usa `DateTime.Now` (hora
  local). **Todo evento de integración fija `DateTime.UtcNow` al construirse**
  (INC-S4, `SISTEMA.md` §12.11).
- `Entity(Guid.Empty)` lanza `ArgumentException`, no `DomainException`. Por eso
  **ningún `Guid` externo llega al constructor de un agregado**: se genera con
  `Guid.NewGuid()` dentro del factory (INC-S2).
- `IRepository<T>` solo tiene `GetByIdAsync` y `AddAsync`. Lo demás va en
  `IRepartidorRepository`, declarada en Domain.
- `builder.Ignore(x => x.DomainEvents)` en la configuración de cada agregado.
- Los agregados lanzan, no devuelven `Result`. No mezcles los dos estilos.

## Reglas propias de BC6

**Dos agregados, y solo dos:** `RutaEntrega` —con `ParadaEntrega` como entidad
hija— y `Repartidor`. `PaqueteRecibido` **no es un agregado**: es un read model
que no hereda de `Entity` y no se accede por `IRepository<T>` (§5.2, §9.4).

**Cinco invariantes NO viven en el agregado**, y no es un olvido (§6.2):

| Código | Vive en | Por qué |
|---|---|---|
| I2 (capacidad) | `CrearRutaCommandHandler` | Compara contra el vehículo de otro agregado |
| I9 | `CrearRutaCommandHandler` + `ReponerPaquetesAlCancelarRutaPolicy` | Es una pregunta sobre el pool completo de paquetes |
| I10 | `CrearRutaCommandHandler` | La fecha del lote vive en el read model |
| I11 | `CrearRutaCommandHandler` + índice único filtrado | Es una consulta sobre la tabla `rutas` entera |
| I12 | `PaqueteRecibidoStore.UpsertAsync` | Es propiedad del adaptador de persistencia |

Sus objetos `Error` **sí** se declaran en `Domain` (`RutaErrors`): el
vocabulario del error es vocabulario de dominio (§6.2, SISTEMA.md §12.1).

**`PARADA_NO_ENCONTRADA` es `Validation` → 400, nunca `NotFound`:** la lanza el
agregado cuando el `paradaId` no está dentro de sus límites, y eso es una
precondición del comando, no una búsqueda. `RUTA_NO_ENCONTRADA` y
`REPARTIDOR_NO_ENCONTRADO` sí son búsquedas de raíz, y sí son 404.

**I5 se garantiza anulando el otro VO.** `ConfirmarEntrega` asigna la constancia
**y pone `Incidencia = null`**; `ReportarIncidencia` hace lo simétrico. Es la
mitad que impide dejar las dos pobladas, y no es una convención.

**BC6 publica los dos eventos que más se han roto en el resto del sistema.**
`EntregaConfirmada` lleva la constancia como objeto **ANIDADO**, con
`coordenadasConfirmacion` presente aunque valga `null`.
`IncidenciaEntregaRegistrada` lleva **`urlFoto`**: son diez campos, no nueve.
Ninguno se aplana, y cada evento publicado lleva su test de contrato (§13.3).

**BC6 no geocodifica.** Las coordenadas llegan resueltas desde BC4 y
`DireccionGeo` las exige (`DIRECCION_COORDENADAS_REQUERIDAS`). De HU-38, BC6
implementa el cálculo del recorrido: haversine + vecino más cercano dentro de
`RutaEntrega.Optimizar(origen)` (§4.1, §12.4).

**Los miembros de `TipoConstancia` y `MotivoIncidencia` van en
`SCREAMING_SNAKE_CASE`**, igual que el contrato: el binding multipart no pasa por
`JsonStringEnumConverter` y `motivo=DIRECCION_NO_ENCONTRADA` no casaría con
`DireccionNoEncontrada` ni ignorando mayúsculas (§5.5, §11.4).

**I12 ignora el duplicado, no hace last-write-wins.** Sobrescribir devolvería a
`POR_ASIGNAR` un paquete ya dentro de una ruta y permitiría asignarlo dos veces,
violando I9 en silencio (§9.3).

## Cómo trabajamos

- Fases en orden. Ninguna empieza si la anterior no compila y no pasa tests.
- **Cada fase tiene su apartado en `docs/GUIA.md`**, con el prompt literal, el reparto
  `[TÚ]` / `[CC]` y la condición de cierre. Cuando te pida trabajar en una fase, lee ese
  apartado antes de proponer nada, y al cerrarla recorre su condición de cierre citándola
  textualmente. No improvises un procedimiento paralelo al que la guía ya escribe.
- Antes de escribir código de una fase, propón el plan y espera aprobación.
- Al terminar un bloque: `dotnet build` y `dotnet test`.
- No ejecutes `git commit` ni `git push`. Prepara el mensaje y para.
- No ejecutes nada que borre datos o migraciones.

## Stack

- .NET 10 (`net10.0`) · C# 13
- `Joseco.DDD.Core` 1.0.2
- MediatR **12.5.0** (CQRS) — versión pineada, no la subas: 13+ exige
  licencia y avisa en cada build (INC-S3)
- EF Core 10 · PostgreSQL 16 · xUnit · Swashbuckle

## Estructura

```
src/Logistica.Domain          RutaEntrega, ParadaEntrega, Repartidor, VOs,
                              eventos, errores
src/Logistica.Application     commands, queries (records), handlers, policies,
                              IntegrationEvents/, PaquetesRecibidos/, puertos
src/Logistica.Infrastructure  DbContext, configuraciones, repositorios, Queries/,
                              UnitOfWork, storage de evidencias, publicador
src/Logistica.WebApi          controllers, Program.cs, middleware de excepciones
tests/Logistica.Domain.Tests       un test por invariante de dominio
tests/Logistica.Application.Tests  orquestación, políticas, contratos
```

## Pruebas (módulo 4)

La estrategia vinculante es `docs/SISTEMA.md` §13 y el §15 de `docs/DESIGN.md`. Cuatro niveles:
unit (Domain + Application con fakes, y serialización de eventos), integración por flujos contra
PostgreSQL real con Testcontainers, y contrato con Pact (message pacts).

- Los tests **se generan y se verifican solo con los orquestadores**: `/unit-tests`,
  `/integration-tests`, `/contract-tests`. Cada uno delega en un agente que escribe y otro que
  verifica sin editar; las reglas de cada nivel están en `.claude/rules/*-tests.md`.
- **Un test nunca se hace pasar tocando `src/`.** Si revela una contradicción con `DESIGN.md`,
  se aplica el protocolo de incoherencias de arriba.
- Aserciones con `Assert` de xUnit. Nada de FluentAssertions (INC-S38).
- Cada test lleva `[Trait("Capa", "Unit" | "Integracion" | "Contrato")]`; los de integración,
  además, `[Trait("Flujo", "F<n>-<nombre>")]` según §15.4 de `DESIGN.md`.
- Estado de avance por nivel: `docs/testing/MAPA-*.md`. Cobertura: `docs/testing/coverage/`.
- Si la herramienta `mcp__codegraph__codegraph_explore` está disponible, úsala para localizar
  código y dependencias antes de recorrer archivos.

## Comandos

```
dotnet build
dotnet test
dotnet test --filter Capa=Unit
dotnet test --filter Capa=Integracion          # requiere Docker
dotnet test --filter Capa=Contrato
powershell -File scripts/test-cobertura.ps1    # reporte en docs/testing/coverage/
dotnet run --project src/Logistica.WebApi     # http://localhost:5060/swagger
dotnet ef migrations add <Nombre> --project src/Logistica.Infrastructure --startup-project src/Logistica.WebApi
dotnet format
```

PostgreSQL en Docker: contenedor `nur-tricenter-postgres`, base `logistica_db`,
puerto 5432. La API escucha en **5060**.

## Commits

Conventional commits en español, imperativo, sin punto final.
`feat(dominio): agregar guard de I8 al optimizar la ruta`
Tipos: feat · fix · test · refactor · docs · chore
