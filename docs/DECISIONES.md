# Decisiones del proyecto — registro cerrado

> Las **cuarenta y dos** contradicciones que se encontraron al contrastar el código real de
> `Joseco.DDD.Core` 1.0.2, el repositorio de referencia del docente
> (`docs/paquete-ejemplo/ms2025-store-inventory`), `docs/SISTEMA.md` y los seis `DESIGN.md`.
> Las treinta y cinco primeras, **decididas y propagadas antes de empezar el desarrollo**; las siete
> de la tercera ronda, al empezar el módulo 4 (Testing). No queda ninguna abierta.
>
> Se lee para saber **qué se decidió y por qué**, con su alternativa descartada: es material
> de defensa, no una lista de pendientes. La numeración `INC-S*` no se renumera nunca.
>
> Algunas entradas resolvían cómo estaba **organizada la documentación** en su momento y citan
> rutas de carpetas que ya no existen. Se conservan por trazabilidad: la decisión sigue siendo
> válida, la ruta no. Las contradicciones que aparezcan **escribiendo código** van al
> `docs/INCOHERENCIAS.md` de tu repositorio, no aquí.

---

## Estado

| # | Qué | Gravedad | Decisión |
|---|---|---|---|
| INC-S1 | `Error.Validation(...)` no existe en el paquete | **Alta** | Cerrada — usar el constructor de `Error` |
| INC-S2 | `Entity(Guid.Empty)` lanza `ArgumentException` → HTTP 500 | **Alta** | **Resuelta** — garantía por construcción **+** rama en el middleware |
| INC-S3 | MediatR 13+ tiene licencia comercial | **Alta** | **Resuelta** — se pinea **12.5.0** |
| INC-S4 | `DomainEvent.OccuredOn` usa `DateTime.Now` (hora local) | Media | **Resuelta** — `UtcNow` explícito en cada evento de integración |
| INC-S5 | Patrón de persistencia del docente ≠ `SISTEMA.md` §12 | Media | **Resuelta — opción A, mapeo directo.** La opción B está escrita entera en `../variante-persistencia-separada/` |
| INC-S6 | `ValidationError` pisa el código de error del catálogo | Baja | **Resuelta** — agrupar en `Application`, desenvolver en la API |
| INC-S7 | Las guías citaban `../docs/SISTEMA.md` | Media | **Resuelta** — corregida de verdad, en los cuatro sitios |
| INC-S8 | Dos andamiajes `.claude/` compitiendo por ser la fuente | **Alta** | **Resuelta** — la v2 se fusiona en `_NUCLEO-02-ANDAMIAJE.md` |
| INC-S9 | `Enum.Parse` sin guarda: el mismo 500 por otra puerta | **Alta** | **Resuelta** — regla transversal en §12.1.1 |
| INC-S10 | C3: ¿un contrato activo por tipo, o solo catering? | Media | **Resuelta** — **solo `CATERING`**, que es lo que RN-19 exige |
| INC-S11 | `Factura`: líneas y porcentaje, o subtotal y fracción | **Alta** | **Resuelta** — líneas **+** porcentaje `[0, 100]`, con la unidad en §8 |
| INC-S12 | La plantilla decía `record struct` y fabricaba tres desviaciones | Baja | **Resuelta** — plantilla corregida a `sealed record` |
| INC-S13 | BC2: diez citas cruzadas mal numeradas y una query fantasma | Media | **Resuelta** — diez sustituciones y «cinco queries», no seis |
| INC-S14 | El `README.md` raíz describe un árbol que no existe | Baja | **Resuelta** — árbol regenerado |
| INC-S15 | `docker-compose.yml` cita «Definición Final §8.5» | Baja | **Resuelta** — referencias a `SISTEMA.md` con su numeración real |
| INC-S16 | `D-01` en BC3, `D1` en los otros cinco | Baja | **Resuelta** — todo a `D-01`…`D-nn` |

**Ninguna queda abierta.** Lo único que sigue pendiente es una pregunta al docente, en INC-S5, que no bloquea nada. La **segunda ronda**, con diecinueve hallazgos más, y la **tercera**, la del módulo 4 de Testing (INC-S36 … INC-S42), están al final de este archivo.

---

## INC-S1 · `Error.Validation(...)` no existe, y `Validation` es el tipo por defecto del dominio

**Fuente A — `SISTEMA.md` §12.1:** «El dominio nunca sabe si un dato faltante es culpa de quien llama o un problema interno, así que `Domain` solo usa `Validation` y `Conflict`.» Es decir, casi todos los errores del dominio son `ErrorType.Validation`.

**Fuente B — `Joseco.DDD.Core` 1.0.2, `Results/Error.cs`:** los factories estáticos son `Failure`, `NotFound`, `Problem` y `Conflict`. **Para `Validation` no hay factory.**

**Por qué importa:** un modelo que escribe `Error.Validation(...)` obtiene un error de compilación y lo «arregla» solo, casi siempre eligiendo `Error.Conflict(...)` porque existe y suena parecido. Eso convierte un HTTP 400 en un 409 sin que nadie lo note. Ya ocurrió: es la desviación **D-03** de `ms-pacientes` §19.

**Decisión:** usar el constructor, siempre, y dejarlo escrito en el `CLAUDE.md` de los seis.

```csharp
new Error("P8_PACIENTE_INACTIVO", "El paciente {pacienteId} esta inactivo",
          ErrorType.Validation, id);
```

No requiere tocar `SISTEMA.md`. Documentado en `_REFERENCIA-JOSECO-DDD-CORE.md` §2.1. **Verificado el 5-sep:** las seis `GUIA-A3-DOMINIO.md`, los seis `CLAUDE.md` y la referencia del paquete ya lo advierten, y ningún `DESIGN.md` usa la forma que no compila.

---

## INC-S2 · `Entity(Guid.Empty)` lanza `ArgumentException`, y el middleware no la capturaba

**Fuente A — `SISTEMA.md` §12.1, redacción anterior:** «Todos los agregados lanzan `DomainException(Error)`, **nunca excepciones nativas de .NET**» y «El middleware **solo captura `DomainException`**.»

**Fuente B — `Joseco.DDD.Core`, `Abstractions/Entity.cs`:**

```csharp
public Entity(Guid id)
{
    if (id == Guid.Empty)
        throw new ArgumentException("Id cannot be empty", nameof(id));
    ...
}
```

**Por qué importa:** un `Guid.Empty` que llegue al constructor de cualquier agregado sale como **HTTP 500 sin cuerpo `{codigo, mensaje}`**, en los seis microservicios. No se puede arreglar dentro del paquete. Estaba documentado como desviación **D-13 únicamente en `ms-pacientes` §19**, cuando afecta a los seis por igual.

**Decisión: A + B.**

- **A — garantía por construcción.** Ningún `Guid` externo llega al constructor: todos los IDs se generan con `Guid.NewGuid()` dentro de los factories y los DTO validan el `Guid` antes. Es lo que el código ya hacía, pero ahora por diseño y escrito.
- **B — red de seguridad en el middleware.** Una rama explícita para `ArgumentException` que devuelve 400 con el formato estándar.

Se descartó envolver `AggregateRoot` en una clase base propia: añade una capa de indirección que hay que explicar en la defensa sin ganar nada que A+B no dé.

**Propagado a:** `SISTEMA.md` §12.1 (la rama del middleware, documentada como la única excepción y con su motivo), §19 de los seis `DESIGN.md` (fila transversal nueva), §18 de los seis, el `CLAUDE.md` común y `_REFERENCIA-JOSECO-DDD-CORE.md`.

---

## INC-S3 · MediatR 13 y posteriores tienen licencia comercial

**Fuente A — `SISTEMA.md` §11, redacción anterior:** «Patrón de aplicación: **CQRS con MediatR 14.x**».

**Fuente B — las seis `GUIA-A3-DOMINIO.md`:** `dotnet add package MediatR --version 12.5.0`.

**Fuente C — Lucky Penny Software:** desde la versión **13.0.0**, MediatR requiere licencia. Las anteriores siguen bajo MIT indefinidamente. Existe una Community License gratuita y **el uso académico califica explícitamente**; lo que no califica es la universidad usándolo para sus sistemas administrativos.

**Por qué importaba:** no era solo la licencia. **Los documentos del proyecto se contradecían entre sí**: quien siguiera la guía instalaba 12.5.0 y quien siguiera el maestro, 14.x. Y con 14.x sin clave registrada, `dotnet build` imprime un aviso de licencia en pantalla durante la defensa.

**Decisión: 12.5.0 en todo.** El proyecto no usa ninguna capacidad exclusiva de 13/14 —`IRequest`, `IRequestHandler`, `INotification` e `IPublisher` están en 12.x— y `Joseco.DDD.Core` 1.0.2 depende de `MediatR.Contracts` 2.0.1, que es la versión que acompaña a la rama 12.

Se descartaron: registrar la Community License en las seis máquinas del grupo (un paso de setup más, sin ganancia), y escribir un dispatcher propio (reescribe el registro de handlers y las políticas de eventos de dominio en los seis, a dos semanas de la defensa).

**Propagado a:** `SISTEMA.md` §11, §18 de los seis `DESIGN.md`, `ms-pacientes/DESIGN.md` §17 fase 5, las seis `GUIA-A3-DOMINIO.md`, los seis `CLAUDE.md` y el bloque común de `_NUCLEO-02-ANDAMIAJE.md`.

> **MassTransit tiene el mismo problema** y llega en el módulo 5: la 8.x es la última libre. Anotado en el `docker-compose.yml` y en `herramientas-ia/04-MODULOS-3-6.md`.

---

## INC-S4 · `OccuredOn` usa `DateTime.Now`, no `DateTime.UtcNow`

**Fuente A — `SISTEMA.md` §12.11:** «Marcas temporales como `DateTime` en ISO-8601.»

**Fuente B — `Joseco.DDD.Core`, `Abstractions/DomainEvent.cs`:** `OccuredOn = DateTime.Now;` — hora local de la máquina, con `Kind = Local`, que serializa sin desplazamiento de zona.

**Por qué importa:** poco mientras los seis microservicios corran en la misma máquina en La Paz. Empieza a importar en cuanto un evento cruce zonas o se compare con una marca generada con `UtcNow` en otro sitio. Y es el tipo de detalle que un docente sí pregunta.

**Decisión:** `SISTEMA.md` §12.11 pasa a exigir **ISO-8601 UTC con sufijo `Z`**, y **todo evento de integración fija `DateTime.UtcNow` explícitamente al construirse**, sin depender de la marca del evento de dominio. Los contratos de §9 lo heredan sin cambiar de forma.

Detalle de escritura: el nombre es **`OccuredOn`**, con una sola `r`. `OccurredOn` no compila.

---

## INC-S5 · El repositorio de referencia del docente usa un modelo de persistencia separado

**Fuente A — `SISTEMA.md` §12:** mapeo directo del dominio con EF Core. Un solo `DbContext`, value objects como owned types, colecciones privadas por backing field, IDs tipados con `ValueConverter`.

**Fuente B — `ms2025-store-inventory`:** dos `DbContext` sobre las mismas tablas. `PersistenceDbContext` con POCOs planos y `[Table]`/`[Column]` es dueño del esquema y de las migraciones; `DomainDbContext` mapea el dominio rico con Fluent API y es el que usan repositorios y `UnitOfWork`.

**Decisión: manda `SISTEMA.md` §12 — mapeo directo.** Motivos:

1. Los seis `DESIGN.md` §14 están escritos sobre mapeo directo, y el código de `project/` está implementado así.
2. La duplicación de dos modelos sobre las mismas tablas falla **en runtime**, no en compilación. Es un riesgo que no compensa a dos semanas de la defensa.
3. El mapeo directo es lo estándar con EF Core moderno desde que existen owned types y `ValueConverter`.

**El patrón del docente no se ignora: se documenta y se escribe entero.** La sección de defensa de cada `GUIA-A4` explica qué es, cuándo se usa —esquema legado, base compartida, dominio que no admite ninguna concesión a la infraestructura— y por qué aquí no se adoptó.

**Y hay más que una explicación.** `../variante-persistencia-separada/` contiene la **opción B completa**: el maestro con §11.1, §12, §13.4 y §14 reescritos, un delta por microservicio con sus tablas reales y sus puntos difíciles, el delta de las guías y el andamiaje alternativo con su hook de verificación. Se escribió el 5 de septiembre de 2026 por dos motivos: para poder defender la elección sabiendo de qué se habla, y porque la pregunta al docente sigue abierta.

Saber nombrar la alternativa y justificar el descarte vale más que haberla copiado. Haberla escrito y **poder enseñar dónde falla el propio repositorio de referencia** —un `ToTable("user", "inventory")` contra un `[Table("user")]` que llevaría a `42P01` en cuanto alguien lo use— vale todavía más.

**Pregunta pendiente para el docente.** Es lo único que queda abierto en todo el proyecto, y no bloquea nada — pero cerrarla decide si la variante B pasa a ser el proyecto:

> En el repositorio de ejemplo (`ms2025-store-inventory`) la persistencia separa `PersistenceDbContext`, dueño del esquema y las migraciones, de `DomainDbContext`, que mapea el dominio rico. Nosotros mapeamos el dominio directamente con EF Core, con un solo `DbContext`, owned types e IDs tipados. ¿Se evalúa la adherencia al patrón del ejemplo, o basta con justificar la decisión de mapeo?

---

## INC-S6 · `ValidationError` reemplaza el código de error del catálogo

`ValidationError` hereda de `Error` fijando `code = "Validation.General"` y `description = "One or more validation errors occurred"`, y guarda los errores originales en la propiedad `Errors`.

Si un handler devuelve un `ValidationError` directamente a la API, el cliente recibe `Validation.General` en vez del código del catálogo §13 —`P8_PACIENTE_INACTIVO` y compañía— y la trazabilidad se pierde justo en el punto donde más se usa.

**Decisión:** `ValidationError.FromResults(...)` solo dentro de `Application`, para agrupar; al cruzar a la API, se devuelve el array `Errors` con sus códigos originales, no el envoltorio. La forma de esa respuesta múltiple está ahora escrita en **`SISTEMA.md` §12.1.2**, que es donde se especifica el middleware — antes solo vivía en este archivo, así que solo la conocía quien lo hubiera leído.

---

## INC-S7 · Ruta de `SISTEMA.md` en las guías

`SISTEMA.md` §15 establece que cada repositorio contiene **`docs/SISTEMA.md`**, copia literal del maestro. La ruta `../docs/SISTEMA.md` solo resuelve trabajando dentro de la carpeta `FINAL-v4`; al abrir Claude Code en el repositorio del microservicio, el modelo se queda sin la fuente vinculante y no avisa.

Se dio por corregida y **no lo estaba**. Seguía viva en cuatro sitios, y el peor era el **prompt modelo de `_NUCLEO-01-METODOLOGIA.md` §7** — el que la metodología pide copiar tal cual.

| Archivo | Estado |
|---|---|
| `guias/README.md` | Corregido, con la nota de dónde vive el original si lees desde `FINAL-v4` |
| `guias/_NUCLEO-01-METODOLOGIA.md` §7 | Corregido en el prompt modelo |
| `ms-catering/DESIGN.md` | El texto visible ya era `docs/SISTEMA.md`; el `href` relativo sigue resolviendo dentro de `FINAL-v4` |
| `ms-produccion-alimentos/DESIGN.md` | Texto visible corregido a `docs/SISTEMA.md` |

---

## INC-S8 · Dos andamiajes `.claude/` compitiendo por ser la fuente

**Fuente A — `herramientas-ia/README.md`:** «`03-ANDAMIAJE-V2.md` sustituye a `_NUCLEO-02-ANDAMIAJE.md` §3 y §5».

**Fuente B — `guias/README.md` y `_NUCLEO-00-SETUP.md` §4:** siguen mandando al lector al `_NUCLEO-02`, con su árbol de tres hooks.

**Por qué importa:** ninguno de los dos documentos sabía que el otro existía, así que quien montara un repositorio elegía por azar. Y las dos configuraciones no son equivalentes: la v2 convierte en **garantías** tres reglas que en la v1 son peticiones al modelo — la regla de dependencias de Clean Architecture, «no terminar con el build roto» y el propio protocolo de incoherencias. Son las tres cosas que más caro salen si fallan, y `_NUCLEO-01` §9 ya argumenta que «un hook no es un recordatorio, es una garantía» sin aplicarlo a ninguna de las tres.

| Hook | v1 | v2 |
|---|---|---|
| `bloquear-destructivos` | PreToolUse | igual |
| `incoherencias-abiertas` | SessionStart | igual |
| `formatear-cs` | PostToolUse · 1–3 s **por edición** | **eliminado**; su trabajo pasa a `cierre-de-turno` |
| `guardian-capas` | — | **PreToolUse**, bloquea un `using` de infraestructura dentro de `Domain` |
| `cierre-de-turno` | — | **Stop**, formatea, compila y no devuelve el turno con el build roto |

**Decisión: fusionar la v2 dentro del núcleo.** `_NUCLEO-02-ANDAMIAJE.md` §3, §5 y §6 son ahora la **única fuente del andamiaje**, con los cinco scripts, la `statusLine` y el criterio `settings.json` vs `settings.local.json`. `herramientas-ia/03-ANDAMIAJE-V2.md` se conserva porque explica **por qué** es así, y su cabecera lo dice: si los dos difieren, manda el núcleo.

Se descartó dejar las dos carpetas enlazadas: mantiene dos `settings.json` divergiendo, que es exactamente el patrón que produjo las seis copias de `SISTEMA.md` que este proyecto ya tuvo que fusionar.

**Propagado a:** `guias/_NUCLEO-02-ANDAMIAJE.md` (§3, §5 nuevo con los cinco hooks, §6 nuevo, §7–§11 renumeradas), `_NUCLEO-00-SETUP.md` §4, `guias/README.md`, `herramientas-ia/03-ANDAMIAJE-V2.md` y `herramientas-ia/README.md`.

---

## INC-S9 · `Enum.Parse` sin guarda: el mismo HTTP 500 por otra puerta

Al revisar las §19 aparece el mismo fallo de INC-S2 en dos microservicios más, y nadie lo había conectado. No es un caso aislado del paquete: **el sistema no tenía una política escrita para las excepciones nativas que sí pueden ocurrir en código propio.**

| Dónde | Qué pasa | Sale como |
|---|---|---|
| `ms-produccion-alimentos` D-02 | `PlanAlimenticioMapper` hace `Enum.Parse<TipoComida>` sin guarda | 500 |
| `ms-contratos-facturacion` D-22 | `Enum.Parse<TipoServicio>(request.TipoServicio, ignoreCase: true)` sin `TryParse` | 500 |
| `ms-pacientes` D-13 | `Entity(Guid.Empty)` del núcleo (INC-S2) | 500 |

**Decisión: regla transversal en `SISTEMA.md` §12.1.1.** Tres puntos: nunca `Parse`, siempre `TryParse`; la conversión fallida lanza `DomainException` con código de catálogo; y ningún `Guid` externo llega al constructor de un agregado.

```csharp
if (!Enum.TryParse<TipoComida>(valor, ignoreCase: true, out var tipo))
    throw new DomainException(new Error(
        "O9_TIPO_COMIDA_DESCONOCIDO",
        $"El tiempo de comida '{valor}' no es valido",
        ErrorType.Validation));
```

Se descartó un catch-all en el middleware: devolvería 400 también a fallos reales de infraestructura, que pasarían a parecer culpa del cliente, y contradice §12.1 de frente.

**Material de defensa:** un 500 en un endpoint de integración es peor que un 400, porque el emisor lo interpreta como fallo transitorio y reintenta indefinidamente un evento que nunca va a entrar.

**Propagado a:** `SISTEMA.md` §12.1.1, el `CLAUDE.md` de los seis, el bloque común de `_NUCLEO-02-ANDAMIAJE.md`, y §18 de los seis `DESIGN.md`.

---

## INC-S10 · C3: ¿un contrato activo por tipo de servicio, o solo para catering?

`ms-contratos-facturacion/DESIGN.md` §19, desviación **D-14**, lo decía literalmente: «unificar el criterio y **documentarlo en el maestro**». Nadie lo hizo, y había tres respuestas distintas en tres sitios.

| Fuente | Dice |
|---|---|
| **RN-19** (maestro) | Unicidad **solo para catering** |
| `DESIGN.md` §6 | Extendía C3 a **todo tipo de servicio**, «por coherencia con el índice `(paciente_id, servicio_tipo)`» |
| Código | `project` valida solo `CATERING`; `NUR-TRICENTER` valida `ASESORAMIENTO` y `CATERING`. Ninguno cubre `EVALUACION` |

El caso que decide: un paciente con contrato `EVALUACION` activo que pide una segunda evaluación. Con la redacción extendida recibía **409**, sin que ninguna HU ni RN lo justificara.

**Decisión: RN-19 manda — unicidad solo para `CATERING`.** El maestro no se toca; se corrige el `DESIGN.md` de BC3, que es quien se salió de la regla. La cadena capacidad → enunciado → RN → invariante que sostiene §16 solo se mantiene si una invariante no exige más de lo que su RN dice, y extender C3 «por coherencia del índice» es dejar que la infraestructura decida una regla de negocio.

**Consecuencias concretas:**

- El índice pasa a ser `ux_contratos_catering_activo_por_paciente`, `UNIQUE (paciente_id) WHERE servicio_tipo = 'CATERING' AND estado = 'ACTIVO'`.
- `C3_CONTRATO_YA_ACTIVO` queda **retirado**; el hueco no se reutiliza (§0.2 del maestro). `C3_CATERING_YA_ACTIVO` cubre el único caso.

---

## INC-S11 · `Factura`: líneas y porcentaje, o subtotal y fracción

La otra desviación de BC3 que decía «alinear, **o cambiar el maestro**» (**D-10**) y quedó sin cerrar. El riesgo no es de diseño, es aritmético.

| | Linaje `project` | Linaje `NUR-TRICENTER` |
|---|---|---|
| Composición | `Factura.lineas` (colección) | `Money subtotal` suelto |
| Impuesto | porcentaje `0 ≤ pct ≤ 100` | fracción `0.13m`, `numeric(5,4)` |
| Maestro §12.2 | lo enumera: «`Factura.lineas`» | — |

```
subtotal = 5085 BOB,  tasa = 13

  porcentaje →  5085 * 13/100  =     661,05  →  total  5 746,05
  fracción   →  5085 * 13      =  66 105,00  →  total 71 190,00
```

**Decisión: líneas + porcentaje**, que es lo que §12.2 del maestro ya enumeraba y lo que sostiene **D-08** (`LineaFacturaId` como entidad hija referenciable). Se descartó el subtotal suelto: pierde el desglose por línea que HU-21 necesita y obliga a tocar el maestro y los seis.

**Y la unidad va en el contrato.** `SISTEMA.md` §8 declara ahora explícitamente que `tasaImpuesto` es un porcentaje en `[0, 100]`, nunca una fracción. Es la clase de dato donde el nombre no basta, igual que `Money` lleva su moneda.

---

## INC-S12 · La plantilla decía `record struct` y fabricaba tres desviaciones falsas

`_PLANTILLA-DESIGN.md` §5.3 decía «tabla de los `record struct` de identidad». Los seis microservicios implementan `sealed record`, y con razón: un id no inicializado es `null` y no `default`, lo que permite validaciones reales como `paqueteId is null`, y evita arrastrar `Nullable<T>` sobre un tipo valor con `ValueConverter` propio, que EF Core mapea peor.

Consecuencia: tres `DESIGN.md` registraban una desviación cuyo origen era una línea de la plantilla, no el código — `ms-pacientes` D-12, `ms-plan-nutricional` D-12 y `ms-produccion-alimentos` D-14. Los tres decían «corregir la redacción de la plantilla». Nadie la corrigió.

**Decisión:** corregida la plantilla, con su porqué. Las tres desviaciones quedan **CERRADAS**.

---

## INC-S13 · BC2: diez citas cruzadas mal numeradas y una query fantasma

El anexo anterior de este archivo listaba cinco. Al verificarlas una a una contra la tabla de §19 aparecieron **diez sitios**: dos más que el anexo no vio. Ninguna toca el modelo ni los contratos —los contratos de §9.2, §9.4, §9.6 y §9.7 coinciden campo a campo con el maestro, y el recuento de §19.4 cuadra con sus dos tablas—, pero llevan al lector a la fila equivocada de §19, que es donde más se usa la trazabilidad.

| Decía | Debía decir | Dónde | Qué es en realidad |
|---|---|---|---|
| `D4` | `D-02` | §3.3, §5.6, §8.1, §9.1 | `D-04` es el puerto 5037 vs 5020 |
| `D7` | `D-08` | §5.7, §14.2 | `D-07` es el N+1 de `RecetaActivaValidator` |
| `D9` | `D-05` | §15.3, cuatro veces | `D-09` es «Completar/Cancelar sin endpoint» |
| `D8` | `D-09` | §18.1 | No estaba en el anexo anterior |

**La query fantasma.** §10.2 lista **cinco** queries y la fase 3 de §17 dice «los cinco records», pero la fase 4 pedía «los **seis** handlers de query». Las guías se escribieron con cinco. **Decisión: son cinco**; el «seis» era un tipeo y §17 fase 4 queda corregida.

**Y el barrido que faltaba, hecho.** El anexo advertía que los otros cinco `DESIGN.md` no habían recibido esta auditoría. Ya la recibieron: `ms-catering` tiene cinco citas cruzadas en el cuerpo y **las cinco son correctas**; `ms-contratos-facturacion`, `ms-logistica-entrega`, `ms-pacientes` y `ms-produccion-alimentos` no citan números de desviación en el cuerpo, así que no pueden equivocarse. **El problema era solo de BC2.**

---

## INC-S14 · El `README.md` raíz describía un árbol que no existe

| Decía | Realidad |
|---|---|
| `docs/_requerimientos-actividad1.txt` | No existe |
| No mencionaba `docs/paquete-ejemplo/` | Es el repo de referencia del docente, y la fuente de INC-S5 |
| No mencionaba `herramientas-ia/` | Cinco documentos, incluida la auditoría del andamiaje |
| «`ms-pacientes/DESIGN.md` · 177 KB» | Las seis cifras estaban desfasadas |
| «Fecha: 2 de septiembre» | `guias/` y `herramientas-ia/` son del 4 |

**Decisión:** árbol regenerado y **fuera las cifras en KB**, que envejecen a cada edición y no informan de nada que el número de secciones no diga mejor.

---

## INC-S15 · `docker-compose.yml` citaba un documento que ya no se llama así

Los comentarios remitían a «Definición Final §8.5» y «§10 y §13 de la Definición Final». Ese documento hoy es `docs/SISTEMA.md`, y los números no coincidían: la comunicación entre microservicios es **§10**, el stack **§11** y la plataforma local **§14**.

Corregido, y aprovechado para anotar allí mismo que **MassTransit 9+ también pasó a licencia comercial** — el bloque comentado de RabbitMQ es lo primero que alguien descomenta en el módulo 5.

---

## INC-S16 · `D-01` en BC3, `D1` en los otros cinco

`ms-contratos-facturacion` numeraba sus 24 desviaciones `D-01`…`D-24`; los otros cinco usaban `D1`…`Dn`. El `README.md` raíz mezclaba las dos formas en la misma tabla, y un `grep D1` en BC3 no devolvía nada.

**Decisión: todo a `D-01` con cero a la izquierda.** Ordena bien hasta `D-24`, que es lo que hacen un `sort` o una tabla de Markdown por defecto — con `D1…D20`, `D10` se cuela entre `D1` y `D2`. §0.2 no lo impide: es una reescritura de formato, no una renumeración, y ninguna desviación cambia de identidad.

Aplicado en los cinco `DESIGN.md` restantes, sus doce guías, cuatro `CLAUDE.md` y el `README.md` raíz. **235 referencias.**

**Verificado:** el recuento total cuadra — 20 + 15 + 14 + 20 + 14 + 24 = **107**, que es la cifra que el `README.md` declara.

---

## Lo que NO es una incoherencia, aunque lo parezca

| Observación | Por qué está bien |
|---|---|
| `IRepository<T>` solo tiene `GetByIdAsync` y `AddAsync` | `SISTEMA.md` §12.7 ya lo contempla: cada BC declara su interfaz propia con los métodos que necesite |
| `Joseco.DDD.Core` es `net8.0` y el proyecto es `net10.0` | Compatible. `SISTEMA.md` §11 lo dice explícitamente |
| Los handlers de query viven en `Infrastructure/`, no en `Application/` | Desviación **consciente** del reparto canónico, justificada en §12.8. Hay que saber defenderla, no corregirla |
| El repo del docente trae husky, commitlint y `Directory.build.props` | Son buenas prácticas de CI, no requisitos del diseño. Opcionales, y mejor después de cerrar la Actividad 3 |
| `DomainEvents` es `ICollection` y no `IReadOnlyCollection` | Limitación del paquete. Se mitiga por disciplina: solo `AddDomainEvent` dentro del agregado |
| La decisión del núcleo DDD aparece en cuatro `§19` distintas | `ms-catering` D-08, `ms-plan-nutricional` D-19, `ms-produccion-alimentos` D-03 y `ms-contratos-facturacion` D-09 registran el `SharedKernel` propio de un linaje y **las cuatro deciden a favor de `Joseco.DDD.Core` 1.0.2**, que es lo que §11 del maestro fija. Sin contradicciones |
| El hueco de `K9` en `ms-catering` | Retirada a propósito y bien propagada: §6.2 del `DESIGN.md`, su `CLAUDE.md`, la guía A3 y el guion de defensa de la A4 dicen lo mismo. El código no se reutiliza |
| Puertos y bases de datos repetidos en cinco documentos | Coinciden todos: `SISTEMA.md` §14, `_NUCLEO-00-SETUP.md` §3, `guias/README.md`, el `README.md` raíz e `init-db/01-create-databases.sql` |

---

## Cómo se usa este archivo a partir de aquí

Este es el registro **del proyecto**, cerrado. Lo que se abre durante la construcción va en el `docs/INCOHERENCIAS.md` de cada repositorio, con el formato y el protocolo de `_NUCLEO-01-METODOLOGIA.md` §3: **Claude Code no resuelve incoherencias, las reporta y para.** El hook `SessionStart` recuerda cuántas siguen abiertas al abrir cada sesión, y `cierre-de-turno` lo repite al final de cada turno.

Un `docs/INCOHERENCIAS.md` con seis entradas resueltas y bien argumentadas es, en la defensa, mejor material que un repositorio sin ninguna: demuestra que leíste la especificación en serio. Este archivo es la prueba de que el equipo ya lo hizo dieciséis veces antes de escribir la primera línea.

---

# Segunda ronda · barrido completo de la carpeta raíz

> Hecho el 5 de septiembre de 2026, después de cerrar las dieciséis primeras y antes de empezar el desarrollo. Cubre `docs/SISTEMA.md`, los seis `DESIGN.md`, `_PLANTILLA-DESIGN.md`, `guias/`, `herramientas-ia/` y `plataforma-local/`, contrastados contra `docs/_caso-de-estudio.md`.
> **Diecinueve hallazgos nuevos, todos aplicados en `FINAL-v5`.** La numeración continúa en `INC-S17`.

| # | Qué | Gravedad | Decisión |
|---|---|---|---|
| INC-S17 | Cinco `DESIGN.md` seguían diciendo «el middleware solo captura `DomainException`» | **Alta** | Corregido en los seis §11 y §17 |
| INC-S18 | El formato de error múltiple de §12.1.2 no existe en ningún `DESIGN.md` | **Alta** | §12.1.2 dice ahora qué se emite hoy y qué el día que se agrupe |
| INC-S19 | `O9_TIPO_COMIDA_DESCONOCIDO` colisiona con la O9 real de BC5 | **Alta** | Se declara **O13** en BC5 y se usa un único código |
| INC-S20 | RN-06: el maestro fija una cuota y BC3 escribe que «no es lo que dice la regla» | **Alta** | Gana la lectura binaria; **RN-06 corregida en el maestro** |
| INC-S21 | §16 no cubría HU-06 ni cuatro RN | Media | Tres filas nuevas y nota de cobertura |
| INC-S22 | §3.1 no atribuía once pares (BC, RN) que sí tienen invariante | Media | Columna completada y criterio de lectura escrito |
| INC-S23 | Campos enumerados tipados como `enum` en el maestro y `string` en cuatro `DESIGN.md` | Media | Regla en §9: el tipo del contrato manda; excepción razonada obliga a `TryParse` |
| INC-S24 | §9.15 fija una clave de idempotencia y tres `DESIGN.md` declaran otra | Media | La clave del evento es del publicador; cada consumidor declara la suya |
| INC-S25 | I2 rechaza por capacidad de vehículo, y ninguna RN lo sostiene | Media | **RN-27 derivada** añadida al maestro; I2 e I11 la citan |
| INC-S26 | Ocho citas a `§10.4` y `§10.5`, secciones que no existían | Baja | §10 numerada de verdad: §10.1 a §10.6 |
| INC-S27 | BC5 nombra dos eventos con sufijo `IntegrationEvent` | Baja | §9 distingue nombre del contrato y nombre del tipo C# |
| INC-S28 | §12.2 no enumeraba `Paciente.entregas` | Baja | Añadida a la lista |
| INC-S29 | El glosario da tres estados de parada y BC6 implementa cuatro | Baja | `EN_CAMINO` definido en el glosario, que es donde nace el término |
| INC-S30 | BC2 afirmaba tener las invariantes más numerosas del sistema | Baja | Corregido: BC3 tiene veintidós |
| INC-S31 | `ms-pacientes` §16.1 anunciaba cuatro filas y presentaba cinco | Baja | Corregido |
| INC-S32 | Conteos y nomenclatura de índices sin fijar | Baja | Conteos corregidos en el `README.md` |
| INC-S33 | **`PowerShell` es una herramienta distinta de `Bash`** y el andamiaje solo cubría `Bash` | **Alta** | Permisos duplicados y matcher `Bash\|PowerShell` |
| INC-S34 | **El hook `Stop` usaba una forma de salida que ya no se interpreta** | **Alta** | `hookSpecificOutput` con `shouldContinue` |
| INC-S35 | La última MediatR bajo MIT es **12.5.0**, no 12.4.1 | Baja | Pineado en 12.5.0 |

---

## INC-S17 · Cinco `DESIGN.md` contradecían a INC-S2 dentro de su propio documento

INC-S2 se propagó a §18 y §19 de los seis, pero **no a su §11**, que es la sección que alguien lee para implementar el middleware. El resultado era una contradicción interna: `ms-pacientes` §18 decía «el middleware traduce `ArgumentException` a 400 (INC-S2)» y su §11 decía «captura **únicamente** `DomainException`», citando §12.1 del maestro como respaldo de lo contrario de lo que §12.1 dice.

**Por qué importa:** quien siga §11 o §17 fase 5 escribe el `catch` sin la rama y reintroduce el HTTP 500 sin cuerpo que INC-S2 dio por resuelto.

**Corregido** en `ms-catering`, `ms-contratos-facturacion`, `ms-logistica-entrega`, `ms-pacientes`, `ms-plan-nutricional` y `ms-produccion-alimentos` (dos sitios en este último), con la nota de **por qué** existe la rama: la lanza el núcleo DDD, que no se puede modificar, y §12.1.1 exige `TryParse` precisamente para que nunca tenga que actuar sobre código propio.

---

## INC-S18 · El formato de error múltiple estaba especificado y no existía

§12.1.2 afirmaba que «el middleware devuelve el array `Errors` con sus códigos originales», y un `grep` de `errores"`, `ValidationError` y `FromResults` sobre los seis `DESIGN.md` devolvía **cero** resultados. BC3 —cuyos dos códigos usa el maestro como ejemplo— declara además «formato del cuerpo de error, **único para toda la API**: `{codigo, mensaje}`».

**Decisión:** decir la verdad en vez de fabricar un formato que las especificaciones no tienen. §12.1.2 dice ahora que **hoy ninguno de los seis agrupa errores**, que la forma `{codigo, mensaje}` es la única que sus APIs emiten, y que esta subsección fija por adelantado cuál será la forma correcta el día que un handler agrupe — para que no se invente entonces. Un microservicio que empiece a agrupar añade la segunda forma a su §11 y un test de contrato que la cubra.

---

## INC-S19 · El maestro inventaba un código de error que ya significaba otra cosa

El ejemplo canónico de §12.1.1 —la regla «nunca `Parse`, siempre `TryParse`» que salió de INC-S9— usaba `O9_TIPO_COMIDA_DESCONOCIDO`. Pero **O9 de BC5 ya significa «la orden es de D-1»**, con su test propio. La nomenclatura de §12.1 es `<INVARIANTE>_<SLUG>`, así que el maestro estaba usando un código con **dos significados distintos**, que es exactamente lo que §0.2 prohíbe. Y la acción recomendada de la desviación `D-02` de BC5 proponía un tercer nombre, `PORCION_TIEMPO_COMIDA_INVALIDO`, así que quien lo corrigiera elegiría uno y rompería la trazabilidad con el otro.

**Decisión:** **O13**, invariante nueva declarada en §6.1 de BC5 y en su catálogo §13. O9 está ocupado y §0.2 impide reutilizarlo. El maestro, la invariante y la acción de `D-02` dicen ahora el mismo código.

O13 no es relleno: es la aplicación local de §12.1.1 en el punto donde BC5 aplana el plan replicado, y su enunciado explica la consecuencia — un tiempo de comida desconocido devolvía **500** en un endpoint de integración, donde el emisor lo lee como fallo transitorio y reintenta para siempre.

---

## INC-S20 · RN-06: la cuota que el maestro fijaba y nadie implementaba

**Fuente A — `SISTEMA.md` RN-06, marcada `E` (literal del enunciado):** «Con catering: 15 días → 1 evaluación quincenal incluida; 30 días → 2.»

**Fuente B — `ms-contratos-facturacion/DESIGN.md` §6.2:** «RN-06 es una lectura binaria, no un contador. […] **No es lo que dice la regla**: mientras haya catering vigente en la fecha, **nunca** se cobra.»

**Fuente C — el código:** ni BC1 (P7) ni BC3 (C7) cuentan nada. Los dos comprueban vigencia.

La segunda frase de RN-06 no la hacía cumplir nadie, y BC3 no lo registraba como desviación ni como fuera de alcance: **contradecía al maestro por escrito**, que es lo que §0.1 prohíbe. Mismo patrón que INC-S10.

**Decisión: gana la lectura binaria, y se corrige RN-06 en el maestro.** El argumento de BC3 es sólido: la frase del enunciado es «los controles se cobran a parte **si es que no se contrató** servicio de catering» — la condición es la contratación, no un cupo. La duración del plan **acota** cuántas quincenales caben (quince días dan margen para una, treinta para dos), pero eso es aritmética de la vigencia, no una cuota que haya que llevar.

Se descartó implementar el contador: exige decidir qué pasa con la evaluación número tres —¿se cobra a tarifa de control adicional?, ¿se rechaza?— y el enunciado no lo responde. Sería alcance inventado. **Coste aceptado, y escrito en §18 de BC3:** un paciente con catering de quince días que registre cinco quincenales no paga ninguna. Se acepta porque quien registra evaluaciones es el nutricionista, no el paciente.

Propagado a RN-06, §1.2, §5.1, el glosario de «Evaluación Quincenal» y §6.2 y §18 de BC3, que ahora cita al maestro en vez de contradecirlo.

---

## INC-S25 · `Vehiculo` existía sin ninguna regla que lo sostuviera

BC6 modela `Vehiculo` con `Tipo`, `Placa` y `CapacidadPaquetes`, tres códigos de error propios y un campo obligatorio en `POST /api/repartidores`. La invariante **I2** rechaza con **409** una asignación que exceda la capacidad, citando **RN-13**. Pero RN-13 dice solo que «el repartidor recibe un conjunto de paquetes etiquetados más un listado», el enunciado no menciona vehículos, y §6.10 declara la flota **fuera de alcance**.

En la defensa es la pregunta fácil: «¿dónde dice el cliente que un repartidor tiene capacidad?».

**Decisión: RN-27 derivada.** «Un repartidor tiene asociado un vehículo con capacidad finita de paquetes y puede tener como máximo una ruta activa a la vez.» Marcada `D` con el mismo criterio que RN-19, RN-20, RN-23 y RN-24: el equipo tuvo que decidir algo que el enunciado dejaba abierto, y ahora está escrito **arriba** antes de programarlo. Su alcance es deliberadamente mínimo —acotar la asignación—, y §6.10 sigue excluyendo nómina, turnos y gestión de flota.

I2 e I11 pasan a citar RN-27 en §6.1 y en §16 de BC6, y el glosario y §5.4 explican de dónde sale. Se descartó reducir `Vehiculo` a dato descriptivo: es más fiel al enunciado literal, pero tira una regla que el grupo ya modeló y probablemente ya programó, a cambio de nada que RN-27 no resuelva mejor.

---

## INC-S23 · El mismo campo, `enum` en el contrato y `string` en el consumidor

§9 del maestro tipa `tipoServicio` como `TipoServicio`, `tipoCambio` como `TipoCambioCalendario`, `motivo` como `MotivoIncidencia` y `tipo` como `TipoComida`. Cuatro `DESIGN.md` los declaran como `string` en el DTO — y `ms-pacientes` los declara tipados **para el mismo contrato**, así que los dos lados de la misma integración no coinciden.

**No es cosmético: es el origen mecánico de las dos desviaciones de 500 que el proyecto ya tenía registradas.** `ms-produccion-alimentos` `D-02` y `ms-contratos-facturacion` `D-22` son literalmente «el DTO trae un `string`, alguien hace `Enum.Parse` sin guarda, sale un 500». Mientras el contrato admita las dos formas, la regla de §12.1.1 se sigue incumpliendo por la misma puerta.

**Decisión:** §9 fija que un campo cuyo tipo es una enumeración de §8 **se declara con ese enum** en el DTO, tanto en el publicador como en el consumidor; `JsonStringEnumConverter` hace el resto y un valor desconocido se rechaza con 400 en el binding, antes de llegar al handler. Un contexto que no quiera modelar vocabulario ajeno puede recibirlo como `string` — pero entonces el `TryParse` con guarda es **obligatorio** y el motivo se escribe en su §9. El subagente `auditor-contratos` lo comprueba.

---

## INC-S24 · Una clave de idempotencia por evento, tres consumidores diciendo otra

§9.15 tiene **una columna de clave por evento**, y tres `DESIGN.md` documentan por separado que esa tabla no describe lo que hacen: BC2 no almacena `contratoId`; BC3 usa `pacienteId + planId`; BC5 indexa por `(pacienteId, fecha)`. Los tres lo llaman «desviación consciente». Es una tabla que los `DESIGN.md` citan catorce veces, y en tres de esas citas se cita para contradecirla.

**Decisión:** la causa es estructural, así que se arregla la estructura, no las tres notas. §9.15 dice ahora que **la clave de la tabla es la del evento, no la de cada consumidor**: identifica una emisión, que es lo que el publicador garantiza. Un evento con varios consumidores tiene tantas unidades de idempotencia como consumidores, porque cada uno decide qué significa «ya lo procesé» según lo que guarda. **Cada consumidor declara su clave efectiva en el §9 de su `DESIGN.md`, y esa es la vinculante para su tabla de procesados.** Lo que la columna sí obliga es que reprocesar la misma emisión no duplique efecto en ninguno.

---

## INC-S33 · `PowerShell` es una herramienta distinta de `Bash`, y el andamiaje solo cubría `Bash`

El hallazgo más caro de los tres de herramientas, y no se veía porque **no da síntoma**.

En Windows, Claude Code expone dos herramientas de shell: `Bash` y `PowerShell`. Un permiso `Bash(git push:*)` en `deny` y un hook con `"matcher": "Bash"` **no cubren** los comandos de PowerShell — que es lo que se ejecuta en una máquina Windows. La regla existe, el hook aparece en `/hooks`, y cuando llega el comando que debía bloquear, no dispara.

**Corregido:** cada regla de permiso está declarada en las dos formas, y el hook de comandos destructivos usa `"matcher": "Bash|PowerShell"`. Dos detalles de la sintaxis de PowerShell que conviene saber y que están escritos en `_NUCLEO-02-ANDAMIAJE.md` §3.1: las reglas **canonicalizan alias**, así que `PowerShell(Get-ChildItem *)` cubre también `gci`, `ls` y `dir`; y Claude Code parsea el AST y comprueba **cada subcomando** de un compuesto por separado.

---

## INC-S34 · El hook `Stop` usaba una forma de salida que ya no se interpreta

`cierre-de-turno.ps1` devolvía `{"decision": "stop", "stopReason": "..."}` para impedir que el turno terminara con el build roto. La forma actual es:

```powershell
@{
    hookSpecificOutput = @{
        hookEventName  = 'Stop'
        shouldContinue = $false
        continueReason = "..."
    }
} | ConvertTo-Json -Depth 5
```

Con la forma antigua el hook corre, el JSON se descarta como no válido y **el turno termina igual**. Es el mismo fallo silencioso que INC-S33: la protección parece montada y no protege. Corregido en el andamiaje y en `_NUCLEO-02-ANDAMIAJE.md` §5.2.

---

## INC-S35 · La última MediatR bajo MIT es 12.5.0

INC-S3 pineó **12.4.1** como «la última MIT». No lo es: **12.5.0**, del 1 de abril de 2025, es la última de la rama 12 antes de que 13.0.0 pasara a licencia comercial en julio de ese año. Las versiones anteriores a la 13 siguen bajo MIT o Apache 2.0 indefinidamente, así que 12.5.0 es libre y trae lo que se arregló después de 12.4.1.

Actualizado a **12.5.0** en los treinta archivos que citaban la versión.

---

## Sobre las licencias, verificado el 5 de septiembre de 2026

**MediatR.** 13.0.0 y posteriores exigen licencia comercial; **anteriores a la 13 siguen bajo MIT o Apache 2.0**, sin fecha de caducidad. La Community License es gratuita para organizaciones con menos de 5 M USD de ingresos y menos de 10 M USD de capital externo, y **el uso académico califica explícitamente**: «instrucción en aula, trabajo de estudiantes, aprendizaje individual, docencia e investigación». Lo que no califica es la universidad usándolo para sus sistemas administrativos. Sin clave registrada, la librería **emite avisos en el build pero funciona con normalidad**: es un recordatorio de renovación, no un bloqueo.

**Conclusión para este proyecto:** con **12.5.0** no hay problema de licencia de ningún tipo, ni avisos en pantalla el día de la defensa. Si algún día este código llega al banco, el uso deja de ser académico y la conversación cambia — pero el pineo ya lo resuelve.

**MassTransit.** La v9 pasó a comercial con su salida en el primer trimestre de 2026. **La v8 sigue abierta**, con parches de seguridad previstos **hasta el final de 2026**. Llega en el módulo 5, así que cuando toque: **se pinea 8.x**, igual que MediatR. Existe además `OpenTransit`, un fork libre de la v8, como plan B si la v8 dejara de mantenerse.

**Lo que hay que hacer, y no es leer esto.** Las condiciones de las dos han cambiado dos veces en dos años. **Verifícalas el día que instales**, no antes ni después.

---

# Tercera ronda · módulo 4 (Testing) — 26 de septiembre de 2026

El módulo 4 pide una capa de testing completa por microservicio: unit tests con cobertura ≥ 80 %, tests de integración con al menos un flujo correcto y uno incorrecto, contract testing con Pact, y el andamiaje de IA que los genera. Contrastado con el maestro y con los seis repositorios, aparecieron siete puntos. Todos decididos y propagados.

| # | Qué | Gravedad | Decisión |
|---|---|---|---|
| INC-S36 | §13 declaraba fuera de alcance justo lo que el módulo 4 exige | **Alta** | **Resuelta** — §13 reescrito con cuatro niveles |
| INC-S37 | El `SISTEMA.md` de `ms-plan-nutricional` divergía del maestro | Media | **Resuelta** — la excepción de `OwnsMany(...).ToJson()` sube al maestro |
| INC-S38 | FluentAssertions 8 en `ms-catering`, y citado en un `DESIGN.md` | Media | **Resuelta** — `Assert` de xUnit en los seis |
| INC-S39 | Pact supone peticiones HTTP; aquí la integración es por eventos | **Alta** | **Resuelta** — message pacts sobre los eventos de §9 |
| INC-S40 | «80 % de cobertura», ¿sobre qué? | **Alta** | **Resuelta** — oficial sobre Domain + Application; combinado como informativo |
| INC-S41 | `ms-logistica-entrega` no tiene tests de serialización de §13.3 | Media | **Resuelta** — se generan con el pipeline de unit tests |
| INC-S42 | Andamiaje de tests: ¿genérico o por microservicio? ¿uno o dos agentes? | Media | **Resuelta** — genérico + mapa específico; escritor y verificador separados |

---

## INC-S36 · §13 declaraba fuera de alcance justo lo que el módulo 4 exige

§13.2 decía «no se levanta PostgreSQL ni Testcontainers» y §13.4 dejaba fuera los tests de Infrastructure, de controllers y end-to-end. La Actividad 2 del módulo exige tests de integración con un flujo correcto y uno incorrecto, y la entrega final dos flujos completos.

**Decisión:** §13 se reescribe con cuatro niveles —unit (13.1–13.3), integración por flujos contra PostgreSQL real con Testcontainers (13.4), contrato con Pact (13.5)— más cobertura (13.6), andamiaje de IA (13.7) y lo que sigue fuera (13.8). La numeración de 13.1 a 13.3 **no cambia**, porque la citan los seis `DESIGN.md`. El §15 de cada `DESIGN.md` se actualiza en consecuencia: el antiguo «15.4 Fuera de alcance» pasa a ser 15.4 Integración, 15.5 Contrato, 15.6 Cobertura y 15.7 Fuera de alcance. El §15.1 del maestro suma las fases 7, 8 y 9.

**Descartado:** mantener la integración solo en Postman. Postman no corre en `dotnet test`, no aporta al reporte de cobertura y no verifica el estado persistido. `demo/demo.http` se conserva para la demo y el video.

---

## INC-S37 · El `SISTEMA.md` de `ms-plan-nutricional` divergía del maestro

La copia de BC2 tenía un párrafo más en §12.3 —la excepción a la regla de owned types para VOs inmutables mapeados con `OwnsMany(...).ToJson()`, fruto de su INC-1— que el maestro y las otras cinco copias no tenían. Es exactamente lo que §0.1 prohíbe: la copia cambió antes que el maestro.

**Decisión:** la excepción es correcta y se incorpora al maestro, que vuelve a copiarse literal a los seis. Las siete copias tienen el mismo hash.

---

## INC-S38 · FluentAssertions 8 en `ms-catering`, y citado en un `DESIGN.md`

`Catering.Domain.Tests` y `Catering.Application.Tests` referencian `FluentAssertions` 8.10.0. Desde la versión 8 la librería es de Xceed, con licencia comercial y gratuita solo para uso no comercial. Los otros cinco repositorios usan `Assert` de xUnit, y el `DESIGN.md` de `ms-pacientes` decía «xUnit con FluentAssertions» sin que su código lo usara.

**Decisión:** `Assert` de xUnit en los seis, en todos los niveles. Se quita la referencia al regenerar los tests de `ms-catering`. Mismo criterio que MediatR (INC-S3): no se introduce una dependencia cuya licencia cambia con el uso.

**Descartado:** pinear FluentAssertions 7.x. Funcionaría, pero deja a un microservicio con un estilo de aserción distinto a los otros cinco y obliga al verificador de IA a conocer dos APIs.

---

## INC-S39 · Pact supone peticiones HTTP; aquí la integración es por eventos

La Actividad 3 pide «al menos dos solicitudes realizadas desde el consumidor y verificadas desde el provider». Entre microservicios no hay ninguna llamada HTTP: se integran por los eventos de §9, hoy simulados con `POST /api/integracion/*` (§10.4).

**Decisión:** **message pacts de PactNet 5.** El consumidor declara el evento que espera y lo pasa por su handler real; el proveedor verifica que su traductor produce ese mensaje. Cada interacción es una «solicitud» del consumidor verificada por el proveedor. El par mínimo es `ms-contratos-facturacion` ← `ms-pacientes` con `PacienteRegistrado` y `EvaluacionRegistrada`; el objetivo son los once pares de §13.5, que son la matriz de §9.15. Encaja además con el módulo 5: cuando llegue RabbitMQ, el contrato del mensaje no cambia.

**Descartado:** pacts HTTP sobre los endpoints de integración. Obligaría a escribir un publicador HTTP que la arquitectura no tiene (§10.3 declara un puerto de publicación, no un cliente HTTP) e invertiría los papeles —el publicador pasaría a ser el «consumidor» del endpoint—, justo al revés del lenguaje del Context Map. **Si el docente exige HTTP**, esta es la alternativa, y se implementa sobre el mismo par mínimo.

---

## INC-S40 · «80 % de cobertura», ¿sobre qué?

La entrega final pide un reporte de cobertura de los unit tests de al menos 80 %. Medido sobre toda la solución, no es alcanzable con unit tests por diseño: los handlers de query viven en Infrastructure (§12.8) y van contra EF Core, y WebApi es composición y controllers.

**Decisión:** el **reporte oficial** mide los unit tests sobre `Domain` + `Application` —donde viven las reglas— con `coverage.runsettings`, umbral 80 % de líneas. Se entrega además un **reporte combinado** unit + integración sobre todos los ensamblados salvo `Migrations` y código generado, como evidencia de que Infrastructure y WebApi quedan cubiertos por §13.4.

**Riesgo:** que el docente espere el 80 % sobre toda la solución con unit tests. El reporte combinado es la respuesta preparada; si no basta, se amplía el filtro y se cubre lo que falte, sin excluir código con lógica.

---

## INC-S41 · `ms-logistica-entrega` no tiene tests de serialización de §13.3

Los otros cinco repositorios tienen sus tests de contrato de eventos; BC6 no tiene ninguno, a pesar de publicar tres eventos y consumir uno.

**Decisión:** se generan con `/unit-tests` como parte de la fase 7. En los otros cinco se **conservan** los existentes al rehacer la suite, porque son la base de los consumer tests de Pact.

---

## INC-S42 · Andamiaje de tests: ¿genérico o por microservicio? ¿uno o dos agentes?

**Decisión, alcance:** reglas, skills, agentes y scripts son **genéricos e idénticos** en los seis repositorios; no nombran agregados ni códigos y los leen de `docs/DESIGN.md`. Lo específico vive en un único archivo por nivel y repositorio, `docs/testing/MAPA-*.md`. Una corrección en las reglas se propaga copiando; nada específico queda enterrado en una skill.

**Decisión, agentes:** **dos por nivel**: el escritor (`test-writer`, `integration-test-writer`, `pact-writer`) y un verificador sin permiso de edición (`*-reviewer`) que devuelve incumplimientos regla por regla. Las correcciones las hace el escritor con ese informe, máximo dos vueltas. Un orquestador por nivel (`/unit-tests`, `/integration-tests`, `/contract-tests`) garantiza que el verificador corre siempre después del escritor.

**Descartado:** un solo agente que escribe y se verifica. Es más simple, pero quien juzga su propio trabajo en el mismo contexto confirma lo que ya cree, y la independencia del verificador es lo que se defiende.

**CodeGraph:** opcional. Si su servidor MCP está conectado, los agentes lo usan para localizar código y dependencias; si no, Grep y Glob.
