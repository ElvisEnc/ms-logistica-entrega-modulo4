using Logistica.Application.Abstractions;

namespace Logistica.Application.Tests.TestSupport;

// Fake en memoria de IEvidenciaStorage (UT-12, MAPA §6): no toca disco, solo registra si se llamo
// y con que parametros, para las aserciones "sin efectos" de UT-06 en las ramas de error de
// ConfirmarEntregaCommandHandler/ReportarIncidenciaCommandHandler.
public sealed class EvidenciaStorageFake : IEvidenciaStorage
{
    public string UrlAGuardar { get; set; } = "https://storage.local/evidencias/evidencia.jpg";
    public bool Llamado { get; private set; }
    public Guid? ParadaIdRecibido { get; private set; }
    public string? NombreArchivoRecibido { get; private set; }

    public Task<string> GuardarAsync(Stream archivo, Guid paradaId, string nombreArchivo, CancellationToken ct = default)
    {
        Llamado = true;
        ParadaIdRecibido = paradaId;
        NombreArchivoRecibido = nombreArchivo;
        return Task.FromResult(UrlAGuardar);
    }
}
