---
layout: default
title: Overview
page_key: overview
description: Run your invoicing, payment records, work tracking, and UK tax estimates in one self-hosted application.
---

<section class="hero">
  <div class="shell hero-grid">
    <div class="hero-copy">
      <p class="eyebrow">Self-hosted accounting workspace</p>
      <h1>Keep the work.<br><em>Lose the paperwork.</em></h1>
      <p class="lede">Accounting brings work records, invoices, bank payments, and practical UK tax estimates into one focused workspace that you control.</p>
      <div class="button-row"><a class="button button-primary" href="{{ '/getting-started/' | relative_url }}">Set up locally <span aria-hidden="true">→</span></a><a class="button button-secondary" href="{{ site.github_url }}">View repository <span aria-hidden="true">↗</span></a></div>
      <dl class="hero-facts"><div><dt>4</dt><dd>connected workflows</dd></div><div><dt>100%</dt><dd>self-hosted</dd></div><div><dt>GBP</dt><dd>ready</dd></div></dl>
    </div>
    <div class="hero-visual">
      <div class="window-frame">
        <div class="window-bar"><span></span><span></span><span></span><small>localhost:3000</small></div>
        <img src="{{ '/assets/images/payments-dashboard.png' | relative_url }}" alt="Accounting payment activity screen showing an imported bank payment">
      </div>
      <p class="visual-note"><span aria-hidden="true">↳</span> Your financial data stays in your PostgreSQL database.</p>
    </div>
  </div>
</section>

<section class="section shell" aria-labelledby="workflow-title">
  <div class="section-heading"><div><p class="eyebrow">One connected workflow</p><h2 id="workflow-title">From bank line to tax estimate</h2></div><p>Each part of the application is useful alone and stronger together. Record the work once, then follow it through invoicing, payment, and reporting.</p></div>
  <ol class="feature-grid">
    <li><span class="feature-number">01</span><h3>Import payments</h3><p>Bring in bank CSV data, filter activity, and keep a dependable record of money in and out.</p></li>
    <li><span class="feature-number">02</span><h3>Record the work</h3><p>Capture dates, clients, projects, descriptions, quantities, and rates as work is completed.</p></li>
    <li><span class="feature-number">03</span><h3>Generate invoices</h3><p>Group eligible work into PDF invoices, monitor payment status, and retain an export history.</p></li>
    <li><span class="feature-number">04</span><h3>Review tax</h3><p>Use earned and received totals to review configurable UK Income Tax and National Insurance estimates.</p></li>
  </ol>
</section>

<section class="section section-dark">
  <div class="shell showcase-grid">
    <div class="showcase-copy"><p class="eyebrow">A useful picture, not a black box</p><h2>See where the numbers come from.</h2><p>The tax view combines completed work and received payments, then shows the inputs and assumptions behind each estimate. It supports planning; it does not pretend to be a submitted return.</p><a class="text-link" href="{{ '/guide/#review-tax-estimates' | relative_url }}">Learn about tax estimates <span aria-hidden="true">→</span></a></div>
    <div class="window-frame window-frame-dark"><div class="window-bar"><span></span><span></span><span></span><small>Tax years</small></div><img src="{{ '/assets/images/tax-estimates.png' | relative_url }}" alt="Accounting tax years screen showing expected pay, earned income, and estimated tax"></div>
  </div>
</section>

<section class="section shell callout" aria-labelledby="start-title">
  <p class="eyebrow">Ready when you are</p>
  <h2 id="start-title">Run the complete stack with one Compose command.</h2>
  <p>Docker Compose starts PostgreSQL, the ASP.NET Core API, and the React interface with sensible local-development defaults.</p>
  <div class="button-row button-row-centred"><a class="button button-primary" href="{{ '/getting-started/' | relative_url }}">Read the setup guide <span aria-hidden="true">→</span></a><a class="button button-secondary" href="{{ site.github_url }}">Browse the source</a></div>
</section>
