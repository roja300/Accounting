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

function Test-FirstRunSetup {
    $temporaryDirectory = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar)
    $temporaryEnvironment = Join-Path $temporaryDirectory "accounting-production-$([Guid]::NewGuid().ToString('N')).env"
    if ([IO.Path]::GetDirectoryName($temporaryEnvironment) -ne $temporaryDirectory) {
        throw "The temporary environment file resolved outside the system temporary directory."
    }

    try {
        $global:AccountingFirstRunResponses = New-Object Collections.Generic.Queue[string]
        @("accounting_user", "FirstRunPassword-2026", "FirstRunPassword-2026") | ForEach-Object {
            $global:AccountingFirstRunResponses.Enqueue($_)
        }
        function global:Read-Host {
            param(
                [string]$Prompt,
                [switch]$AsSecureString
            )

            $response = $global:AccountingFirstRunResponses.Dequeue()
            if ($AsSecureString) {
                return ConvertTo-SecureString $response -AsPlainText -Force
            }
            return $response
        }

        & (Join-Path $repositoryRoot "start-production.ps1") -EnvironmentFile $temporaryEnvironment -ValidateOnly -NoBrowser
        Assert-Condition (Test-Path -LiteralPath $temporaryEnvironment -PathType Leaf) "First-run setup must create the environment file."

        $settings = @{}
        Get-Content -LiteralPath $temporaryEnvironment | Where-Object { $_ -match '^[A-Z_]+=' } | ForEach-Object {
            $parts = $_ -split '=', 2
            $settings[$parts[0]] = $parts[1]
        }
        Assert-Condition ($settings["POSTGRES_DB"] -eq "accounting") "First-run setup must use the default database name."
        Assert-Condition ($settings["POSTGRES_USER"] -eq "accounting_user") "First-run setup must save the database user."
        Assert-Condition ($settings["POSTGRES_PASSWORD"] -eq "FirstRunPassword-2026") "First-run setup must save the confirmed password."
        Assert-Condition ($settings["FRONTEND_PORT"] -eq "3000") "First-run setup must use the default frontend port."

        $bytes = [IO.File]::ReadAllBytes($temporaryEnvironment)
        $hasUtf8Bom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
        Assert-Condition (-not $hasUtf8Bom) "First-run setup must write UTF-8 without a byte-order mark."
    } finally {
        Remove-Item Function:\global:Read-Host -ErrorAction SilentlyContinue
        Remove-Variable AccountingFirstRunResponses -Scope Global -ErrorAction SilentlyContinue
        if (Test-Path -LiteralPath $temporaryEnvironment) {
            Remove-Item -LiteralPath $temporaryEnvironment
        }
    }
}

Test-FirstRunSetup
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
