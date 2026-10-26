$ErrorActionPreference = "Stop"

function Assert-Condition {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$productionEnvironment = Join-Path $repositoryRoot "src/.env.production.example"
$productionCompose = Join-Path $repositoryRoot "src/compose.production.yaml"
$developmentEnvironment = Join-Path $repositoryRoot "src/.env.development"
$developmentCompose = Join-Path $repositoryRoot "src/compose.yaml"

& (Join-Path $repositoryRoot "start-production.ps1") -EnvironmentFile $productionEnvironment -ValidateOnly

$productionJson = & docker compose --env-file $productionEnvironment --file $productionCompose config --format json
if ($LASTEXITCODE -ne 0) {
    throw "Could not render the production Compose configuration."
}
$production = $productionJson | ConvertFrom-Json

Assert-Condition ($production.name -eq "accounting-production") "The production stack must use an isolated project name."
Assert-Condition ($production.services.backend.environment.ASPNETCORE_ENVIRONMENT -eq "Production") "The production API must run in the Production environment."
Assert-Condition ($production.services.frontend.build.target -eq "production") "The production stack must build the nginx frontend target."
Assert-Condition ($production.services.frontend.ports.Count -eq 1) "Only one frontend port should be published."
Assert-Condition ($production.services.frontend.ports[0].target -eq 80) "The production frontend must serve HTTP through nginx."
Assert-Condition ($null -eq $production.services.backend.ports) "The production API must not publish a host port."
Assert-Condition ($null -eq $production.services.postgresql.ports) "The production database must not publish a host port."

$developmentJson = & docker compose --env-file $developmentEnvironment --file $developmentCompose config --format json
if ($LASTEXITCODE -ne 0) {
    throw "Could not render the development Compose configuration."
}
$development = $developmentJson | ConvertFrom-Json

Assert-Condition ($development.services.backend.environment.ASPNETCORE_ENVIRONMENT -eq "Development") "The existing API development environment must be preserved."
Assert-Condition ($development.services.frontend.ports[0].target -eq 5173) "The development stack must continue to publish Vite."
Assert-Condition (@($development.services.frontend.volumes | Where-Object type -eq "bind").Count -eq 1) "The development frontend source bind mount must be preserved."
Assert-Condition ($development.services.backend.ports[0].target -eq 8080) "The development API host port must be preserved."
Assert-Condition ($development.services.postgresql.ports[0].target -eq 5432) "The development database host port must be preserved."

Write-Host "Production and development Compose topology checks passed."
