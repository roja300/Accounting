[CmdletBinding()]
param(
    [string]$EnvironmentFile = (Join-Path $PSScriptRoot "src/.env.production"),
    [switch]$ValidateOnly
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "Docker is required. Install or start Docker Desktop, then try again."
}

$environmentFilePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($EnvironmentFile)
if (-not (Test-Path -LiteralPath $environmentFilePath -PathType Leaf)) {
    throw "Production environment file not found at '$environmentFilePath'. Copy 'src/.env.production.example' to 'src/.env.production', set a secure POSTGRES_PASSWORD, and try again."
}

$composeFile = Join-Path $PSScriptRoot "src/compose.production.yaml"
$composeArguments = @(
    "compose",
    "--env-file", $environmentFilePath,
    "--file", $composeFile
)

if ($ValidateOnly) {
    $composeArguments += @("config", "--quiet")
} else {
    $composeArguments += @("up", "--build", "--detach", "--wait")
}

& docker $composeArguments
if ($LASTEXITCODE -ne 0) {
    throw "Docker Compose exited with code $LASTEXITCODE."
}

if (-not $ValidateOnly) {
    Write-Host "Accounting production containers are running."
}
