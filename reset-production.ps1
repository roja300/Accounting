[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = "Medium")]
param(
    [string]$EnvironmentFile = (Join-Path $PSScriptRoot "src/.env.production"),
    [switch]$Force
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "Docker is required. Install or start Docker Desktop, then try again."
}

$environmentFilePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($EnvironmentFile)
if (-not (Test-Path -LiteralPath $environmentFilePath -PathType Leaf)) {
    $environmentFilePath = Join-Path $PSScriptRoot "src/.env.production.example"
}

$warningMessage = "DESTRUCTIVE RESET: This permanently deletes all Accounting production database data, containers, networks, volumes, and locally built production images. This cannot be undone."
Write-Warning $warningMessage

if (-not $Force) {
    $confirmation = Read-Host "Type RESET to continue"
    if ($confirmation -cne "RESET") {
        Write-Host "Production reset cancelled. Nothing was removed."
        return
    }
}

$target = "Accounting production containers, network, volumes, database data, and locally built images"
if (-not $PSCmdlet.ShouldProcess($target, "Permanently reset")) {
    return
}

$composeFile = Join-Path $PSScriptRoot "src/compose.production.yaml"
& docker compose `
    --env-file $environmentFilePath `
    --file $composeFile `
    down `
    --volumes `
    --rmi local `
    --remove-orphans

if ($LASTEXITCODE -ne 0) {
    throw "Docker Compose exited with code $LASTEXITCODE."
}

Write-Host "Accounting production containers, network, volumes, database data, and locally built images were removed."
