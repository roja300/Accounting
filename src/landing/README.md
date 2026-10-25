# Accounting landing site

This directory contains the Jekyll source for the Accounting GitHub Pages site. Page content is written in Markdown; the shared HTML shell lives in `_layouts`, and presentation assets live under `assets`.

## Run locally

### Rider

Select **Serve Landing Site** from Rider's run configuration menu and run it. The shared PowerShell configuration starts the Docker-based Jekyll preview and exposes it at `http://localhost:4000/`. Docker Desktop must be running; Ruby does not need to be installed locally. If the configuration is unavailable, enable Rider's PowerShell plugin and reload the project.

Stop the run configuration to stop and remove the preview container. The first run downloads the Ruby image and installs the Jekyll dependencies into the reusable `accounting-landing-gems` Docker volume.

### Ruby and Bundler

If Ruby and Bundler are installed locally, run:

```powershell
Set-Location src/landing
bundle install
$landingOutput = Join-Path $env:TEMP "accounting-landing-site"
bundle exec jekyll serve --baseurl "" --destination $landingOutput
```

Open `http://localhost:4000/`. The deployment workflow overrides `baseurl` with the path supplied by GitHub Pages.

## Deploy with GitHub Pages

Before the first deployment, enable GitHub Pages for the repository:

1. Open the repository on GitHub and select **Settings**.
2. Select **Pages** under **Code and automation**.
3. Under **Build and deployment**, set **Source** to **GitHub Actions**.

After this one-time setup, pushes to `master` that change `src/landing` or the Pages workflow build, test, and deploy the site. The workflow can also be started manually from the repository's **Actions** tab.

The built-in `GITHUB_TOKEN` cannot enable Pages on a repository where no Pages site exists. Do not add `enablement: true` to `actions/configure-pages` unless the workflow is also intentionally supplied with a separate token that has the required repository administration and Pages permissions.

## Validate

```powershell
bundle exec jekyll build --strict_front_matter
bundle exec ruby test/site_test.rb
```
