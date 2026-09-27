---
paths:
  - "tests/**/*ContractTests*/**/*.cs"
---

# Reglas de contract tests (módulo 4)

Genéricas para los seis microservicios de NUR-TRICENTER. No nombran pares, eventos,
Value Objects ni códigos de error de ningún microservicio en particular: esos datos se
leen en tiempo de ejecución de `docs/SISTEMA.md` §13.5/§9 y `docs/DESIGN.md` §15.5 del
repositorio donde se esté trabajando. Cada regla tiene un ID estable (`CT-xx`) que no se
renumera.

## CT-01 · Mecanismo por defecto: message pacts de PactNet 5, sin broker

El mecanismo por defecto es un *message pact* de PactNet 5 (`SISTEMA.md` §13.5,
INC-S39). Sin Pact Broker: el intercambio entre repositorios es multi-repo, por copia de
archivo (`scripts/publicar-pacts.ps1`). Nombre de archivo:
`pacts/<Consumidor>-<Proveedor>.json`.

**Por qué:** `SISTEMA.md` §13.5 — «la comunicación entre contextos es por eventos, así
que el contrato se prueba con message pacts de PactNet 5»; «intercambio de archivos:
multi-repo sin broker».

## CT-02 · Excepción: Pact HTTP para verificación de proveedor bloqueada por un bug
##      verificado de la librería

Si la verificación de proveedor de un message pact (`PactVerifier(...).WithMessages(...)`)
falla por una causa ajena al código propio —por ejemplo, un bug documentado y abierto en
el repositorio oficial de PactNet, no un defecto del test o del traductor—, no se fuerza
el test a pasar ni se toca `src/`. Se registra como incoherencia (protocolo de
`CLAUDE.md`), citando el issue exacto de la librería. Si la incoherencia se resuelve
autorizando una alternativa, la única alternativa válida es **Pact HTTP**
(`WithHttpInteractions`/`PactVerifier.WithHttpEndpoint`) montado sobre un **shim de
solo test**: un host HTTP mínimo, definido enteramente en el proyecto de contract tests,
que expone la ejecución real del traductor de evento de dominio a evento de integración
detrás de un endpoint HTTP synthetic. Nunca se añade un endpoint de producción nuevo en
`src/` para esto, y nunca se inventa un tercer mecanismo.

**Por qué:** acuerdo de proyecto tras verificar en código que la verificación de
proveedor de PactNet 5 en modo mensaje puede estar bloqueada por un bug ajeno al
repositorio (ver `docs/INCOHERENCIAS.md` de cada repositorio para el caso concreto). La
alternativa HTTP ya estaba prevista como salida de emergencia en `DECISIONES.md`
(INC-S39): «si el docente exige HTTP, esta es la alternativa, y se implementa sobre el
mismo par mínimo» — aquí se reutiliza esa misma alternativa técnica, con una causa
distinta (un bug verificado, no una exigencia del docente).

## CT-03 · El shim HTTP nunca sustituye al traductor real

Cuando CT-02 aplica, el shim HTTP no reimplementa la traducción de evento de dominio a
evento de integración: construye el evento de dominio real, lo publica a través del
mismo mecanismo de notificación que usa la aplicación en producción (de forma que el
handler real y el traductor real se ejecuten sin cambios), captura lo que ese traductor
produjo con un publicador de eventos de integración sustituido por uno que captura en
memoria, y devuelve ese resultado serializado como cuerpo de la respuesta HTTP. El shim
no depende de infraestructura de persistencia real si el traductor bajo prueba no la
necesita.

**Por qué:** el objetivo de un test de contrato es probar el traductor real y su
serialización real (regla equivalente a IT-04 en integración, pero orientada a
publicación en vez de a consumo); un shim que solo devuelve un JSON fijo no prueba nada
distinto de un test de serialización ya cubierto por `DESIGN.md` §15.3.

## CT-04 · Lado consumidor: el mensaje pasa por el handler/endpoint real

El lado consumidor de un message pact —o, bajo CT-02, el lado que la actividad exige
que quede «verificado desde el provider»— pasa el mensaje esperado por el
handler/endpoint real que lo procesa en producción (`POST /api/integracion/*` o el
command/handler equivalente), no solo por una aserción de forma del JSON. Si ese
handler depende de un puerto de persistencia, ese puerto se puede sustituir por una
implementación que captura en memoria, exactamente igual que en integración (IT-04):
lo que se prueba aquí es el contrato de entrada (deserialización, construcción de VOs,
validaciones de forma), no el efecto de persistencia, que ya cubre el nivel de
integración.

**Por qué:** `SISTEMA.md` §13.5 — «el consumidor... lo pasa por su handler real».

## CT-05 · Lado proveedor: el traductor real, con las `JsonSerializerOptions` reales

El lado proveedor produce el evento con su traductor real (el mismo tipo/método que usa
el publicador de producción) y lo serializa con las MISMAS `JsonSerializerOptions` que
usa el publicador real (nunca unas creadas ad hoc para el test, aunque tengan el mismo
valor: se copian literalmente de la fuente real y se referencia esa fuente en un
comentario).

**Por qué:** `SISTEMA.md` §13.5 — «el proveedor... produce el evento con su traductor
real y lo serializa con las mismas JsonSerializerOptions del publicador».

## CT-06 · Campos opcionales cubiertos en ambos estados

Cada matcher de un campo opcional cubre el caso con el campo presente y el caso con el
campo en `null`, igual que exige `DESIGN.md` §15.3 para los tests de serialización.
Pact no sustituye esos tests, los complementa: prueba el acuerdo entre las dos partes,
no la forma interna de cada lado por separado.

**Por qué:** `DESIGN.md` §15.5 — «los tests de serialización de §15.3 siguen
existiendo: prueban el lado propio; Pact prueba el acuerdo entre los dos».

## CT-07 · `[Trait("Capa", "Contrato")]`

Toda clase de test de este nivel lleva `[Trait("Capa", "Contrato")]`.

**Por qué:** es el trait que permite `dotnet test --filter Capa=Contrato`
(`CLAUDE.md`, «Pruebas (módulo 4)»).

## CT-08 · Pactos "bootstrap": rotulado obligatorio

Un pacto autorado en este repositorio en nombre de un consumidor o proveedor externo
que todavía no tiene su propia infraestructura de contract tests («bootstrap») se
rotula sin ambigüedad como transitorio: un comentario en el archivo de test que lo
autora, y una nota explícita en el MAPA de contract tests de este repositorio. Nunca se
presenta como una verificación real de un tercero. Cuando el otro repositorio
implemente su propia suite, su pacto real sustituye al bootstrap sin que este
repositorio necesite cambiar su lado.

**Por qué:** acuerdo de proyecto — un pacto bootstrap sin rotular podría leerse como
si un tercero ya lo hubiera verificado, lo cual sería falso y rompería la garantía que
Pact existe para dar.

## CT-09 · Nunca se toca `src/` para que un test pase

Un test de este nivel nunca se hace pasar modificando código de `src/`. Si un test
revela que el código no cumple `docs/SISTEMA.md`/`docs/DESIGN.md`, se deja escrito con
`[Fact(Skip = "INC-<n>")]` y se aplica el protocolo de incoherencias de `CLAUDE.md`.

**Por qué:** `SISTEMA.md` §13.5 — «un pact que el proveedor no verifica es un contrato
roto: se corrige en el lado que se desvió de §9, nunca editando el .json a mano» —
mismo principio aplicado al código que produce o consume el evento.

## CT-10 · Ubicación en espejo

Una clase de test por interacción o grupo de interacciones del mismo par, en
`tests/<Proyecto>.ContractTests/Consumidor/` (cuando este repositorio es el
consumidor del par) o `tests/<Proyecto>.ContractTests/Proveedor/` (cuando es el
proveedor). El shim de CT-02, si existe, vive en una carpeta `Testing/` o
`Infraestructura/` aparte, nunca mezclado con las clases de test.

**Por qué:** mismo principio que IT-10/UT-16 — localizar sin buscar dónde vive el test
de un par, esta vez organizado por rol (consumidor/proveedor) en vez de por flujo.

## CT-11 · Aserciones solo con `Assert` de xUnit

Ninguna aserción usa FluentAssertions ni ninguna otra librería de aserciones.

**Por qué:** `CLAUDE.md`, «Pruebas (módulo 4)» — «Aserciones con `Assert` de xUnit.
Nada de FluentAssertions (INC-S38)».

## CT-12 · Qué no se prueba en este nivel

No se escriben tests de contrato para: un bus de mensajería real (Kafka, RabbitMQ,
etc. — la integración real hoy es in-process, `SISTEMA.md` §10.1), un Pact Broker real
(la decisión vinculante es sin broker), ni condiciones de carrera entre publicaciones
concurrentes. Eso queda fuera de alcance del módulo 4 según `docs/DESIGN.md` §15.7 (o
la sección equivalente que ese documento use para delimitar lo que el módulo 4 no
cubre).

**Por qué:** `docs/DESIGN.md` §15.7 fija ese límite para el nivel de integración, y el
mismo principio aplica al nivel de contrato: probarlo aquí duplicaría esfuerzo sobre un
alcance que el propio plan de pruebas excluye.
