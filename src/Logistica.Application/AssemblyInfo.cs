using System.Runtime.CompilerServices;

// Los commands son `internal sealed record` (§10.1 DESIGN.md): no forman parte de la API
// publica del ensamblado. WebApi los construye para enviarlos por MediatR desde el controller
// del mismo proceso, y los tests de Application los construyen directamente.
[assembly: InternalsVisibleTo("Logistica.Application.Tests")]
[assembly: InternalsVisibleTo("Logistica.WebApi")]
[assembly: InternalsVisibleTo("Logistica.ContractTests")]
