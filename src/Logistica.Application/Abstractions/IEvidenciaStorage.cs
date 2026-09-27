namespace Logistica.Application.Abstractions;

// §18.3 del DESIGN.md: puerto sobre almacenamiento local hoy, sustituible por S3/MinIO despues
// sin tocar Domain ni Application. Devuelve la URL relativa servida como archivo estatico.
public interface IEvidenciaStorage
{
    public Task<string> GuardarAsync(Stream archivo, Guid paradaId, string nombreArchivo, CancellationToken ct = default);
}
