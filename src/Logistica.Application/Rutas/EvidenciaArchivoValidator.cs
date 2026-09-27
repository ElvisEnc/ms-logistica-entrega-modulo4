using Joseco.DDD.Core.Results;
using Logistica.Domain.Rutas;

namespace Logistica.Application.Rutas;

// D-09 (§19.2 DESIGN.md), INC-1 Opcion A: el guard de extension/tamano vive en Application,
// compartido por ConfirmarEntregaCommandHandler y ReportarIncidenciaCommandHandler -- los dos
// unicos que reciben un Stream de evidencia. Se comprueba ANTES de llamar a
// IEvidenciaStorage.GuardarAsync, igual que EVIDENCIA_ARCHIVO_REQUERIDO.
internal static class EvidenciaArchivoValidator
{
    private static readonly string[] ExtensionesPermitidas = [".jpg", ".jpeg", ".png"];
    private const long TamanoMaximoBytes = 5 * 1024 * 1024;

    public static void Validar(Stream archivo, string? nombreArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo ?? string.Empty);
        if (!ExtensionesPermitidas.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new DomainException(RutaErrors.EvidenciaExtensionNoPermitida());

        if (archivo.CanSeek && archivo.Length > TamanoMaximoBytes)
            throw new DomainException(RutaErrors.EvidenciaTamanoExcedido());
    }
}
