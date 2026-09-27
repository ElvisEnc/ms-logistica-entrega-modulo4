# Referencia — `Joseco.DDD.Core` 1.0.2

> API **real** del paquete, leída del código fuente en `docs/paquete-ejemplo/joseco.ddd.core/`.
> Cópiala como `docs/JOSECO-DDD-CORE.md` en cada repositorio: el `CLAUDE.md` la referencia y evita que Claude invente métodos que el paquete no tiene.

```xml
<PackageReference Include="Joseco.DDD.Core" Version="1.0.2" />
```

`net8.0`, consumible desde `net10.0` sin cambios. Única dependencia: `MediatR.Contracts` 2.0.1.
Dos namespaces: `Joseco.DDD.Core.Abstractions` y `Joseco.DDD.Core.Results`.

---

## 1. Lo que el paquete trae — la superficie completa

### `Abstractions`

```csharp
public abstract class Entity
{
    public Guid Id { get; protected set; }
    public ICollection<DomainEvent> DomainEvents { get; }   // ← mutable, ojo

    public Entity(Guid id);        // lanza ArgumentException si id == Guid.Empty
    protected Entity();            // para EF Core

    public void AddDomainEvent(DomainEvent domainEvent);
    public void ClearDomainEvents();
}

public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id);
    protected AggregateRoot();
}

public abstract record DomainEvent : INotification
{
    public Guid Id { get; set; }
    public DateTime OccuredOn { get; set; }   // ← "Occured", sin la doble r. Y DateTime.Now
}

public interface IRepository<TEntity> where TEntity : AggregateRoot
{
    Task<TEntity?> GetByIdAsync(Guid id, bool readOnly = false);
    Task AddAsync(TEntity entity);
}

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
```

### `Results`

```csharp
public enum ErrorType { Failure = 0, Validation = 1, Problem = 2, NotFound = 3, Conflict = 4 }

public record Error
{
    public static readonly Error None;
    public static readonly Error NullValue;

    public Error(string code, string structuredMessage, ErrorType type, params object[]? args);

    public string Code { get; }
    public string Description { get; }          // structuredMessage con los {placeholders} sustituidos
    public string StructuredMessage { get; }
    public ErrorType Type { get; }

    public static Error Failure (string code, string structuredMessage, params string[]? args);
    public static Error NotFound(string code, string structuredMessage, params string[]? args);
    public static Error Problem (string code, string structuredMessage, params string[]? args);
    public static Error Conflict(string code, string structuredMessage, params string[]? args);
}

public sealed record ValidationError : Error
{
    public ValidationError(Error[] errors);
    public Error[] Errors { get; }
    public static ValidationError FromResults(IEnumerable<Result> results);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success();
    public static Result<TValue> Success<TValue>(TValue value);
    public static Result Failure(Error error);
    public static Result<TValue> Failure<TValue>(Error error);
}

public class Result<TValue> : Result
{
    public TValue Value { get; }                              // lanza si IsFailure
    public static implicit operator Result<TValue>(TValue? value);
    public static Result<TValue> ValidationFailure(Error error);
}

public class DomainException(Error Error) : Exception { public Error Error { get; } }
```

**Eso es todo.** No hay `Specification`, no hay `IReadRepository`, no hay `Result.Map`, no hay `Result.Bind`, no hay `Error.Validation(...)`. Si un prompt te devuelve código que usa cualquiera de esos, no compila.

---

## 2. Las siete trampas

### 2.1 No existe `Error.Validation(...)` — y es el `ErrorType` que más se usa

Hay factories estáticos para `Failure`, `NotFound`, `Problem` y `Conflict`. **Para `Validation` no hay.** Y `docs/DESIGN.md` §13 asigna `ErrorType.Validation` a la mayoría de las precondiciones, porque es lo que traduce a HTTP 400.

```csharp
// ✗ No compila
Error.Validation("PACIENTE_INACTIVO", "El paciente {0} esta inactivo", id);

// ✓ Así se escribe
new Error("PACIENTE_INACTIVO", "El paciente {pacienteId} esta inactivo", ErrorType.Validation, id);
```

Consecuencia práctica: es tentador usar `Error.Conflict(...)` porque «existe y se parece». Eso convierte un 400 en un 409 y rompe el contrato de API de `SISTEMA.md` §12.1. Ya pasó una vez en `ms-pacientes` (desviación D3 de su §19). **El `ErrorType` se copia del catálogo §13, nunca del error de al lado.**

Y recuerda la regla de reparto de `SISTEMA.md` §12.1: desde `Domain` solo salen `Validation` y `Conflict`. `NotFound` lo añade `Application` cuando busca la **raíz del agregado** por repositorio y no la encuentra. Una precondición sobre una entidad hija que ya vive dentro del agregado —un día que el plan no tiene, una dirección que la libreta no tiene— es `Validation`, no `NotFound`.

Lo razonable es una clase estática de errores por agregado, y ahí dentro el `new Error(...)`:

```csharp
public static class PacienteErrors
{
    public static Error Inactivo(Guid id) =>
        new("PACIENTE_INACTIVO", "El paciente {pacienteId} esta inactivo",
            ErrorType.Validation, id);
}
```

### 2.2 `DomainEvents` es `ICollection`, no `IReadOnlyCollection`

Cualquiera puede hacer `entidad.DomainEvents.Clear()` desde fuera del agregado. El paquete es así; no lo puedes cambiar. Lo que sí puedes es **no aprovecharlo**: los eventos se añaden solo con `AddDomainEvent` dentro del agregado, y solo el `UnitOfWork` los recoge y llama a `ClearDomainEvents`.

Y en EF Core hay que ignorarlos **en la configuración de todos los agregados**:

```csharp
builder.Ignore(x => x.DomainEvents);
```

Si se olvida, EF intenta mapear `DomainEvent` y la migración sale con tablas que nadie pidió.

### 2.3 `Entity(Guid id)` lanza `ArgumentException` con `Guid.Empty`

Es una excepción nativa de .NET saliendo de la capa de dominio, justo lo que `SISTEMA.md` §12.1 prohíbe. No se puede evitar desde fuera del paquete. Dos consecuencias:

- Los IDs se generan **siempre** dentro de los factories con `Guid.NewGuid()`. Ningún `Guid` llega al constructor desde un DTO sin validar antes.
- El middleware de excepciones tiene que tener una rama para `ArgumentException` que devuelva 400, no un 500. Es una red de seguridad, no el camino normal.

### 2.4 `OccuredOn` usa `DateTime.Now`, no `UtcNow`

El constructor de `DomainEvent` fija `OccuredOn = DateTime.Now`: hora local de la máquina, sin zona. Si el `DESIGN.md` o `SISTEMA.md` de tu microservicio dicen que las marcas de tiempo van en UTC, tienes dos opciones y **la elección es tuya, no de Claude**:

```csharp
// A) Sobrescribir al construir el evento
var evento = new PacienteRegistrado(id, ...) { OccuredOn = DateTime.UtcNow };

// B) Aceptar la hora local y documentarlo en DESIGN.md §18 como decisión
```

Y ojo con el nombre: es `OccuredOn`, con una sola `r`. Escribir `OccurredOn` no compila y es un error que se comete constantemente.

### 2.5 `IRepository<T>` solo tiene dos métodos

`GetByIdAsync` y `AddAsync`. No hay `Update`, no hay `Delete`, no hay listados. Cada bounded context declara su propia interfaz en `Domain`, extendiendo la del paquete:

```csharp
public interface IPacienteRepository : IRepository<Paciente>
{
    Task<Paciente?> ObtenerPorIdentificacionAsync(string identificacion);
    Task<bool> ExisteIdentificacionAsync(string identificacion);
}
```

No hace falta `Update`: EF Core con change tracking persiste las mutaciones del agregado cargado al hacer `CommitAsync`.

### 2.6 En este proyecto los agregados **lanzan**, no devuelven `Result`

El paquete ofrece `Result`/`Result<T>`, y es un patrón perfectamente válido. **Pero `SISTEMA.md` §12.1 fija la otra convención:** los agregados lanzan `DomainException(Error)` y un middleware único traduce `Error.Type` a HTTP, en vez de repetir la comprobación del resultado en cada controller.

```csharp
// ✓ La convención de este proyecto
if (Estado != EstadoPaciente.Activo)
    throw new DomainException(PacienteErrors.Inactivo(Id));   // P8 (RN-04)

// ✗ Válido en C#, pero no es lo que este proyecto hace
if (Estado != EstadoPaciente.Activo)
    return Result.Failure(PacienteErrors.Inactivo(Id));
```

No mezcles los dos estilos dentro de un microservicio: el middleware solo captura `DomainException`, así que un `Result` fallido que nadie inspecciona se convierte en un éxito silencioso. Si en algún punto el `DESIGN.md` de tu microservicio usa `Result` en la firma de un método del dominio, eso es una incoherencia con §12.1: regístrala y pregunta.

`Result` sí tiene sitio en `Application` cuando un handler necesita componer varios fallos antes de decidir; ahí `ValidationError.FromResults(...)` es útil. Pero la frontera con la API sigue siendo la excepción.

### 2.7 La conversión implícita de `Result<TValue>` se traga los `null`

```csharp
public static implicit operator Result<TValue>(TValue? value) =>
    value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
```

Un `return null;` en un método que devuelve `Result<Paciente>` **no da error de compilación**: da un `Failure` con `Error.NullValue`, código `"General.Null"`, que no está en tu catálogo §13 y que la API traducirá a lo que sea que haga el middleware con un error desconocido. Devuelve siempre el error del catálogo de forma explícita.

### 2.8 El constructor de `Result` valida y lanza

`new Result(true, unErrorQueNoEsNone)` lanza `ArgumentException`. En la práctica no importa si usas los factories estáticos, que es lo que hay que hacer. No construyas `Result` a mano.

---

## 3. Cómo se usa el paquete en este proyecto — el patrón canónico

```csharp
// Domain/Pacientes/Paciente.cs
using Joseco.DDD.Core.Abstractions;
using Joseco.DDD.Core.Results;

public sealed class Paciente : AggregateRoot
{
    private readonly List<Evaluacion> _evaluaciones = new();
    public IReadOnlyCollection<Evaluacion> Evaluaciones => _evaluaciones.AsReadOnly();

    public string Identificacion { get; private set; }
    public EstadoPaciente Estado { get; private set; }

    private Paciente() { }                                   // EF Core

    private Paciente(Guid id, string identificacion) : base(id)
    {
        Identificacion = identificacion;
        Estado = EstadoPaciente.Activo;
    }

    public static Paciente Registrar(string identificacion)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
            throw new DomainException(PacienteErrors.IdentificacionRequerida());

        var paciente = new Paciente(Guid.NewGuid(), identificacion);
        paciente.AddDomainEvent(new PacienteRegistrado(paciente.Id, identificacion));
        return paciente;
    }

    public void RegistrarEvaluacion(DateOnly fecha, TipoEvaluacion tipo)
    {
        // P8 (RN-04): no se evalúa a un paciente inactivo
        if (Estado != EstadoPaciente.Activo)
            throw new DomainException(PacienteErrors.Inactivo(Id));

        _evaluaciones.Add(Evaluacion.Crear(fecha, tipo));
    }
}
```

Lo que hay que reconocer en ese código, porque es lo que se pregunta en la defensa:

- El estado se muta **solo desde métodos del agregado**, nunca desde fuera.
- La invariante se comprueba **antes** de mutar y cita su código y su regla de negocio.
- El fallo es `DomainException(Error)` con un error del catálogo, según §12.1.
- La entidad hija se crea desde la raíz; `Evaluacion.Crear` no es accesible desde un handler.
- El evento se registra dentro del método que produjo el cambio.

---

## 4. Lo que le dices a Claude Code sobre el paquete

En el `CLAUDE.md`:

```markdown
## Joseco.DDD.Core

La API real del paquete está en `docs/JOSECO-DDD-CORE.md`. Léela antes de
usar cualquier tipo del paquete. En particular:

- NO existe `Error.Validation(...)`. Para ErrorType.Validation se usa el
  constructor: `new Error(codigo, mensaje, ErrorType.Validation, args)`.
- El ErrorType de cada error sale del catálogo de docs/DESIGN.md §13. No se
  copia del error de al lado porque "se parece".
- `DomainEvent.OccuredOn` se escribe con una sola r y usa DateTime.Now.
- `IRepository<T>` solo tiene GetByIdAsync y AddAsync. Los métodos propios
  van en la interfaz del repositorio de este BC, en Domain.
- Los agregados **lanzan `DomainException(Error)`** (SISTEMA.md §12.1); no
  devuelven `Result`. No mezcles los dos estilos.
- Desde Domain solo salen ErrorType.Validation y Conflict.
- Nunca devuelvas `null` desde un método que retorna `Result<T>`.
- `builder.Ignore(x => x.DomainEvents)` en la configuración de cada agregado.
```
