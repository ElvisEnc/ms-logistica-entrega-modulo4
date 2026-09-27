<#
    SISTEMA.md §13.5: intercambio de pacts multi-repo sin broker. Copia los pacts generados por
    este repositorio (donde ms-logistica-entrega es CONSUMIDOR o donde autora un pacto
    "bootstrap" en nombre de un proveedor/consumidor externo, CT-08) a la carpeta pacts/ de cada
    repositorio hermano real.

    IMPORTANTE: al momento de escribir este script, ninguno de los repos hermanos tiene todavia
    su propio proyecto de contract tests (Tarea 3 de este modulo). Este script SI copia los
    archivos a rutas reales en disco, pero el lado receptor aun no tiene nada que los consuma.
    Cuando ese repo implemente su Tarea 3, su propio pacto real sustituira al que este script
    copio (CT-08).
#>

$repoRoot = Split-Path -Parent $PSScriptRoot
$pactsOrigen = Join-Path $repoRoot "tests\Logistica.ContractTests\pacts"
$proyectosRoot = Join-Path (Split-Path -Parent $repoRoot) "proyectos"

if (-not (Test-Path $pactsOrigen)) {
    Write-Error "No existe $pactsOrigen. Corre 'dotnet test --filter Capa=Contrato' primero para generar los pacts."
    exit 1
}

# <archivo generado> -> <repo hermano que debe recibirlo>
$destinos = @{
    "ms-logistica-entrega-ms-produccion-alimentos.json" = "ms-produccion-alimentos"
    "ms-pacientes-ms-logistica-entrega.json"            = "ms-pacientes"
    "ms-catering-ms-logistica-entrega.json"             = "ms-catering"
}

foreach ($archivo in $destinos.Keys) {
    $origenArchivo = Join-Path $pactsOrigen $archivo
    $repoDestino = Join-Path $proyectosRoot $destinos[$archivo]

    if (-not (Test-Path $origenArchivo)) {
        Write-Warning "No se generó $archivo todavía (ejecuta /contract-tests o dotnet test primero). Se omite."
        continue
    }

    if (-not (Test-Path $repoDestino)) {
        Write-Warning "No existe el repositorio hermano $repoDestino en disco. Se omite $archivo."
        continue
    }

    $carpetaPactsDestino = Join-Path $repoDestino "pacts"
    New-Item -ItemType Directory -Force -Path $carpetaPactsDestino | Out-Null

    $destinoArchivo = Join-Path $carpetaPactsDestino $archivo
    Copy-Item -Path $origenArchivo -Destination $destinoArchivo -Force
    Write-Host "Copiado: $archivo -> $carpetaPactsDestino"
}

Write-Host ""
Write-Host "Recordatorio: el repositorio receptor todavia no tiene un proyecto de contract tests" -ForegroundColor Yellow
Write-Host "que consuma estos archivos (verificado en esta sesion). La copia deja el archivo listo" -ForegroundColor Yellow
Write-Host "para cuando ese repositorio implemente su propia Tarea 3." -ForegroundColor Yellow
