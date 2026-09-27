[CmdletBinding()]
param(
    [int]$Umbral = 80
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$testResultsDir = Join-Path $repoRoot 'TestResults'
$coverageDir = Join-Path $repoRoot 'docs\testing\coverage'
$runsettings = Join-Path $repoRoot 'coverage.runsettings'

if (Test-Path $testResultsDir) {
    Remove-Item -Recurse -Force $testResultsDir
}

Push-Location $repoRoot
try {
    dotnet test --settings $runsettings --collect:"XPlat Code Coverage" --results-directory $testResultsDir
}
finally {
    Pop-Location
}

$coverageFiles = Get-ChildItem -Path $testResultsDir -Filter 'coverage.cobertura.xml' -Recurse -ErrorAction SilentlyContinue

if (-not $coverageFiles -or $coverageFiles.Count -eq 0) {
    Write-Host "No se genero ningun coverage.cobertura.xml en $testResultsDir. Revisa que los proyectos de test tengan al menos un test y que dotnet test no haya fallado." -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $coverageDir)) {
    New-Item -ItemType Directory -Force -Path $coverageDir | Out-Null
}

$reportsArg = ($coverageFiles.FullName -join ';')

reportgenerator "-reports:$reportsArg" "-targetdir:$coverageDir" "-reporttypes:Html;TextSummary;MarkdownSummaryGithub"

if ($LASTEXITCODE -ne 0) {
    Write-Host "reportgenerator termino con codigo $LASTEXITCODE." -ForegroundColor Red
    exit 1
}

$summaryPath = Join-Path $coverageDir 'Summary.txt'

if (-not (Test-Path $summaryPath)) {
    Write-Host "No se encontro $summaryPath tras ejecutar reportgenerator." -ForegroundColor Red
    exit 1
}

$summaryContent = Get-Content -Path $summaryPath -Raw

$lineMatch = [regex]::Match($summaryContent, 'Line coverage:\s*([\d.,]+)%')
$branchMatch = [regex]::Match($summaryContent, 'Branch coverage:\s*([\d.,]+)%')

if (-not $lineMatch.Success) {
    Write-Host "No se pudo leer el porcentaje de lineas en $summaryPath." -ForegroundColor Red
    exit 1
}

$lineCoverage = [double]::Parse($lineMatch.Groups[1].Value, [System.Globalization.CultureInfo]::InvariantCulture)
$branchCoverage = $null
if ($branchMatch.Success) {
    $branchCoverage = [double]::Parse($branchMatch.Groups[1].Value, [System.Globalization.CultureInfo]::InvariantCulture)
}

Write-Host ""
Write-Host "Cobertura de lineas: $lineCoverage%"
if ($null -ne $branchCoverage) {
    Write-Host "Cobertura de ramas: $branchCoverage%"
}
else {
    Write-Host "Cobertura de ramas: no disponible en el reporte"
}
Write-Host "Umbral requerido (lineas): $Umbral%"
Write-Host ""

if ($lineCoverage -lt $Umbral) {
    Write-Host "Cobertura de lineas ($lineCoverage%) por debajo del umbral ($Umbral%)." -ForegroundColor Red
    exit 1
}

Write-Host "Cobertura de lineas OK." -ForegroundColor Green
exit 0
