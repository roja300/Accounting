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
      <h1>Accounting without the clutter.</h1>
      <p class="lede">Keep work records, invoices, bank payments, and practical UK tax estimates together in one straightforward workspace that you control.</p>
      <div class="button-row"><a class="button button-primary" href="{{ '/getting-started/' | relative_url }}">Set up locally <span aria-hidden="true">→</span></a><a class="button button-secondary" href="{{ site.github_url }}">View repository <span aria-hidden="true">↗</span></a></div>
    </div>
    <div class="hero-visual">
      <div class="window-frame"><img src="{{ '/assets/images/payments-dashboard.png' | relative_url }}" alt="Accounting payment activity screen showing an imported bank payment"></div>
    </div>
  </div>
</section>

<section class="section shell" aria-labelledby="workflow-title">
  <div class="section-heading"><div><p class="eyebrow">One connected workflow</p><h2 id="workflow-title">From work to payment</h2></div><p>Record information once, then follow it through invoicing, payment, and tax planning.</p></div>
  <ol class="feature-grid">
    <li><span class="feature-number">01</span><h3>Import payments</h3><p>Bring in bank CSV data, filter activity, and keep a dependable record of money in and out.</p></li>
    <li><span class="feature-number">02</span><h3>Record the work</h3><p>Capture dates, clients, projects, descriptions, quantities, and rates as work is completed.</p></li>
    <li><span class="feature-number">03</span><h3>Generate invoices</h3><p>Group eligible work into PDF invoices, monitor payment status, and retain an export history.</p></li>
    <li><span class="feature-number">04</span><h3>Review tax</h3><p>Use earned and received totals to review configurable UK Income Tax and National Insurance estimates.</p></li>
  </ol>
</section>

<section class="section shell callout" aria-labelledby="start-title">
  <p class="eyebrow">Run it yourself</p>
  <h2 id="start-title">Start the complete stack with Docker Compose.</h2>
  <p>Use the setup guide to run PostgreSQL, the API, and the React interface locally.</p>
  <div class="button-row button-row-centred"><a class="button button-primary" href="{{ '/getting-started/' | relative_url }}">Read the setup guide <span aria-hidden="true">→</span></a><a class="button button-secondary" href="{{ site.github_url }}">Browse the source</a></div>
</section>
