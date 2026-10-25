[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$landingRoot = Split-Path -Parent $PSScriptRoot

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "Docker is required to serve the landing site. Install or start Docker Desktop, then try again."
}

Write-Host "Starting the Accounting landing site at http://localhost:4000/"
Write-Host "The first run downloads Ruby and installs the Jekyll dependencies."

docker run --rm `
    -p 4000:4000 `
    --mount "type=bind,source=$landingRoot,target=/site" `
    --mount "type=volume,source=accounting-landing-gems,target=/usr/local/bundle" `
    -w /site `
    ruby:3.3 `
    bash -lc "bundle install && bundle exec jekyll serve --host 0.0.0.0 --port 4000 --baseurl '' --destination /tmp/accounting-landing-site"

if ($LASTEXITCODE -ne 0) {
    throw "The landing site container exited with code $LASTEXITCODE."
}
