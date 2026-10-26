[CmdletBinding()]
param(
    [string]$EnvironmentFile = (Join-Path $PSScriptRoot "src/.env.production"),
    [switch]$ValidateOnly,
    [switch]$NoBrowser
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
    $dockerArguments = $composeArguments + @("config", "--quiet")
} else {
    $dockerArguments = $composeArguments + @("up", "--build", "--detach", "--wait")
}

& docker $dockerArguments
if ($LASTEXITCODE -ne 0) {
    throw "Docker Compose exited with code $LASTEXITCODE."
}

if (-not $ValidateOnly) {
    $publishedEndpoints = @(& docker @composeArguments port frontend 80)
    if ($LASTEXITCODE -ne 0 -or $publishedEndpoints.Count -eq 0) {
        throw "Accounting is running, but the published frontend port could not be determined."
    }

    $publishedEndpoint = $publishedEndpoints[0].Trim()
    if ($publishedEndpoint -notmatch ':(\d+)$') {
        throw "Accounting is running, but the published frontend port could not be determined."
    }

    $applicationUrl = "http://localhost:$($Matches[1])/"
    Write-Host "Accounting production containers are running at $applicationUrl"

    if (-not $NoBrowser) {
        try {
            Start-Process $applicationUrl
        } catch {
            Write-Warning "Accounting is running, but the browser could not be opened automatically. Open $applicationUrl manually."
        }
    }
}
