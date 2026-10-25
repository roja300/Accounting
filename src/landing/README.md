# Accounting landing site

This directory contains the Jekyll source for the Accounting GitHub Pages site. Page content is written in Markdown; the shared HTML shell lives in `_layouts`, and presentation assets live under `assets`.

## Run locally

Install Ruby and Bundler, then run:

```powershell
Set-Location src/landing
bundle install
bundle exec jekyll serve
```

Open `http://127.0.0.1:4000/Accounting/`. The deployment workflow overrides `baseurl` with the path supplied by GitHub Pages.

## Validate

```powershell
bundle exec jekyll build --strict_front_matter
bundle exec ruby test/site_test.rb
```
