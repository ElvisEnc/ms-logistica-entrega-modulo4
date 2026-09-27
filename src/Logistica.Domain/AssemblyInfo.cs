using System.Runtime.CompilerServices;

// ParadaEntrega solo expone sus mutadores como internal (§5.2 DESIGN.md): toda operacion
// entra por RutaEntrega, nunca directamente. Los tests de dominio necesitan verlos para
// probar el nivel-parada de I4/I5/I8 sin pasar por el agregado.
[assembly: InternalsVisibleTo("Logistica.Domain.Tests")]
