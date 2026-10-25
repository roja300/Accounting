# GitHub Page

*Date: 25/10/2026* <br>
*Author: Rojaware*<br>

## Overview

This CR outlines the creation of a GitHub page as part of this repository to demonstrate the apps functionality, and provides a guide.

### Functional Specification (FS)

1. The website should provide an overview.
2. The website should provide a getting started guide.
3. The website should demonstrate how to use the application.
4. The website should include screenshots for the application.
5. The website should include an empty repository.

### Non-functional Specification (NFS)

1. Use Jekyll and use Markdown for content.
2. Put the website in src/landing.

## Notes

- N/A

### Codex Response

#### [2026-10-25 17:23] Added the GitHub Pages Site

Added a responsive Jekyll site under `src/landing` with a product overview, Docker-based getting-started guide, step-by-step user guide, application screenshots captured with synthetic data, source repository links, local-development instructions, a custom accessible layout, and a not-found page. Added source-structure tests and a GitHub Actions workflow that tests, builds, packages, and deploys the site to GitHub Pages when landing-site files change on `master`.

##### Comments

1. Interpreted “include an empty repository” as “include the repository” because the application repository is necessarily populated; the site links prominently to the public source repository from its navigation, hero, guide, and footer.
2. Validation passed in a Ruby 3.3 container: 4 source tests with 25 assertions and a strict Jekyll production build. Browser checks passed for all pages at desktop width and for the overview at 390 px, including successful routes, loaded images, expected headings, and no horizontal overflow.
