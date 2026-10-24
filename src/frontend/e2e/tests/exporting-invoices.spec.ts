import { expect, test } from '@playwright/test';

const invoice = {
	id: 'ed652b89-8923-4c68-920c-2e32bbb4150f',
	invoiceNumber: 'INV-20261019-ABC123',
	clientId: 'b31f5928-64b0-4dd7-aad1-3a3ca55158ed',
	clientName: 'Acme Ltd',
	issueDate: '2026-10-19',
	dueDate: '2026-11-18',
	currency: 'GBP',
	subtotal: 125,
	notes: 'Payment due within 30 days.',
	status: 'exported',
	lines: [
		{
			workRecordId: '9eb37a9e-3f15-4213-a9fd-0ba004af2842',
			description: 'Monthly bookkeeping',
			project: 'Retainer',
			value: 125,
		},
	],
	createdAt: '2026-10-19T12:00:00Z',
};

test('should present empty work and invoice results as full-width panels', async ({
	page,
}) => {
	await page.setViewportSize({ width: 1200, height: 800 });
	await page.route('**/api/invoices**', async (route) => {
		const url = new URL(route.request().url());
		await route.fulfill({
			json: url.pathname.endsWith('/options')
				? { clients: [], projects: [] }
				: {
						items: [],
						page: 1,
						pageSize: 9,
						totalItems: 0,
						totalPages: 0,
					},
		});
	});
	await page.route('**/api/exported-invoices**', (route) =>
		route.fulfill({
			json: {
				items: [],
				page: 1,
				pageSize: 9,
				totalItems: 0,
				totalPages: 0,
			},
		}),
	);

	for (const [path, heading] of [
		['/invoices', 'No work records yet'],
		['/invoices/exported', 'No exported invoices yet'],
	] as const) {
		await page.goto(path);
		const panel = page
			.getByText(heading, { exact: true })
			.locator('xpath=ancestor::*[@data-slot="card"]');
		await expect(panel).toBeVisible();
		const dimensions = await panel.boundingBox();
		expect(dimensions?.width).toBeGreaterThan(900);
		expect(dimensions?.height).toBeGreaterThanOrEqual(256);
	}
});

test('should generate an invoice from the exported invoices page using named options', async ({
	page,
}) => {
	let request: Record<string, unknown> | undefined;
	const projectId = 'dd0c3ad8-e41c-47f4-9c15-bfb0c5a132b4';
	await page.route('**/api/exported-invoices**', async (route) => {
		const url = new URL(route.request().url());
		if (url.pathname.endsWith('/options')) {
			await route.fulfill({
				json: {
					clients: [
						{ id: invoice.clientId, name: invoice.clientName },
					],
					projects: [
						{
							id: projectId,
							name: 'Retainer',
						},
					],
				},
			});
			return;
		}
		if (route.request().method() === 'GET') {
			await route.fulfill({
				json: {
					items: [],
					page: 1,
					pageSize: 9,
					totalItems: 0,
					totalPages: 0,
				},
			});
			return;
		}
		request = route.request().postDataJSON() as Record<string, unknown>;
		await route.fulfill({ status: 201, json: invoice });
	});

	await page.goto('/invoices/exported');
	await page.getByRole('button', { name: 'Generate invoice' }).click();
	const dialog = page.getByRole('dialog', { name: 'Generate invoice' });
	await expect(dialog.getByLabel('Client')).toHaveAccessibleDescription(
		'Only clients with work ready to invoice are shown.',
	);
	const clientSelect = dialog.getByLabel('Client');
	await clientSelect.click();
	await page.getByRole('option', { name: 'Acme Ltd' }).click();
	await expect(clientSelect).toContainText('Acme Ltd');
	await expect(clientSelect).not.toContainText(invoice.clientId);
	const projectSelect = dialog.getByLabel('Project');
	await projectSelect.click();
	await page.getByRole('option', { name: 'Retainer' }).click();
	await expect(projectSelect).toContainText('Retainer');
	await expect(projectSelect).not.toContainText(projectId);
	await dialog.getByRole('button', { name: 'Generate invoice' }).click();

	await expect(
		page.getByRole('heading', { name: 'Invoice ready' }),
	).toBeVisible();
	const generatedDownload = page.getByRole('link', {
		name: `Download ${invoice.invoiceNumber} PDF`,
	});
	await expect(generatedDownload).toContainText('Download PDF');
	await expect(generatedDownload.locator('svg')).toBeVisible();
	expect(request?.clientId).toBe(invoice.clientId);
	expect(request?.projectId).toBe(projectId);
	expect(request?.currency).toBe('GBP');
});

test('should update payment status and confirm before deleting an invoice', async ({
	page,
}) => {
	let updatedStatus = '';
	let deleteCalls = 0;
	await page.route('**/api/exported-invoices**', async (route) => {
		const request = route.request();
		if (request.method() === 'PATCH') {
			updatedStatus = (request.postDataJSON() as { status: string })
				.status;
			await route.fulfill({
				json: { ...invoice, status: updatedStatus },
			});
			return;
		}
		if (request.method() === 'DELETE') {
			deleteCalls += 1;
			await route.fulfill({ status: 204 });
			return;
		}
		await route.fulfill({
			json: {
				items: [invoice],
				page: 1,
				pageSize: 9,
				totalItems: 1,
				totalPages: 1,
			},
		});
	});

	await page.goto('/invoices/exported');
	await expect(page.getByText('INV-20261019-ABC123')).toBeVisible();
	const cardDownload = page.getByRole('link', {
		name: `Download ${invoice.invoiceNumber} PDF`,
	});
	await expect(cardDownload).toContainText('PDF');
	await expect(cardDownload.locator('svg')).toBeVisible();
	await page.getByRole('button', { name: 'Payment received' }).click();
	await expect.poll(() => updatedStatus).toBe('paymentReceived');
	await page
		.getByRole('button', { name: 'Delete INV-20261019-ABC123' })
		.click();
	const dialog = page.getByRole('dialog', { name: 'Delete this invoice?' });
	await expect(dialog).toContainText('marked as deleted');
	await dialog.getByRole('button', { name: 'Delete invoice' }).click();
	await expect.poll(() => deleteCalls).toBe(1);
});
