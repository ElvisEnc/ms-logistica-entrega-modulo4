using Logistica.Application.Abstractions;
using Microsoft.Extensions.Hosting;

namespace Logistica.Infrastructure.Storage;

// §18.3 DESIGN.md: almacenamiento local tras el puerto IEvidenciaStorage, sustituible por
// S3/MinIO despues sin tocar Domain ni Application.
//
// INC-1 (docs/INCOHERENCIAS.md), Opcion A: el guard de extension/tamano de la desviacion D-09
// (§19.2 DESIGN.md) vive en Application (EvidenciaArchivoValidator), no aqui. Esta clase recibe
// el stream ya validado.
public sealed class LocalEvidenciaStorage(IHostEnvironment environment) : IEvidenciaStorage
{
    private const string CarpetaEvidencias = "evidencias";

    public async Task<string> GuardarAsync(Stream archivo, Guid paradaId, string nombreArchivo, CancellationToken ct = default)
    {
        // La carpeta se resuelve con IHostEnvironment.ContentRootPath (§11.7 DESIGN.md): mezclar
        // esto con una ruta relativa hace que las evidencias se guarden bien pero devuelvan 404
        // al consultarlas, justo el sintoma que RN-16 no puede permitirse.
        var carpeta = Path.Combine(environment.ContentRootPath, CarpetaEvidencias);
        Directory.CreateDirectory(carpeta);

        var extension = Path.GetExtension(nombreArchivo);
        // {paradaId}-{timestamp}{ext} (§18.3 DESIGN.md): agrupa en disco todos los archivos de
        // una misma parada bajo el mismo identificador que aparece en el evento de integracion.
        var nombreFinal = $"{paradaId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{extension}";
        var rutaCompleta = Path.Combine(carpeta, nombreFinal);

        await using var destino = File.Create(rutaCompleta);
        await archivo.CopyToAsync(destino, ct);

        return $"/{CarpetaEvidencias}/{nombreFinal}";
    }
}
