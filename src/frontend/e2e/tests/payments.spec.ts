import { expect, type Page, test } from '@playwright/test';

const importId = '7f598074-0ba6-4da2-b795-848e7f14195e';
const payment = {
	id: '4bbcbe0f-e779-4a23-9fbe-8175a2a5afab',
	importId,
	importFileName: 'october-statement.csv',
	transactionAt: '2026-10-24T14:30:00Z',
	description: 'October salary',
	counterparty: 'Example Consulting Ltd',
	reference: 'PAY-2026-10',
	amount: 2500,
	balance: 8420.1,
	currency: 'GBP',
};
const paymentImport = {
	id: importId,
	fileName: 'october-statement.csv',
	rowCount: 1,
	importedAt: '2026-10-25T12:00:00Z',
};

async function routePaymentData(page: Page) {
	await page.route('**/api/payments/imports', async (route) => {
		if (route.request().method() === 'POST') {
			await route.fulfill({ status: 201, json: paymentImport });
			return;
		}
		await route.fulfill({ json: [paymentImport] });
	});
	await page.route('**/api/payments?*', async (route) => {
		if (route.request().method() === 'DELETE') {
			await route.fulfill({ status: 204 });
			return;
		}
		await route.fulfill({
			json: {
				items: [payment],
				page: 1,
				pageSize: 25,
				totalItems: 1,
				totalPages: 1,
			},
		});
	});
}

test('should display and filter imported payments', async ({ page }) => {
	let listRequest = '';
	await routePaymentData(page);
	await page.route('**/api/payments?*', async (route) => {
		listRequest = route.request().url();
		await route.fulfill({
			json: {
				items: [payment],
				page: 1,
				pageSize: 25,
				totalItems: 1,
				totalPages: 1,
			},
		});
	});

	await page.goto('/payments');
	await expect(
		page.getByRole('heading', { name: 'Payment activity' }),
	).toBeVisible();
	await expect(
		page.getByRole('cell', { name: /October salary/ }),
	).toContainText('PAY-2026-10');
	await expect(
		page.getByRole('cell', { name: 'Example Consulting Ltd' }),
	).toBeVisible();
	await expect(page.getByRole('cell', { name: '£2,500.00' })).toBeVisible();

	await page.getByLabel('Search').fill('salary');
	await page.getByRole('combobox', { name: 'Direction' }).click();
	await page.getByRole('option', { name: 'Money in' }).click();
	await page.getByRole('button', { name: 'Apply filters' }).click();

	await expect.poll(() => listRequest).toContain('search=salary');
	expect(listRequest).toContain('direction=incoming');
});

test('should import a bank csv and report completion', async ({ page }) => {
	await routePaymentData(page);

	await page.goto('/payments');
	await page.getByRole('button', { name: 'Import CSV' }).click();
	const dialog = page.getByRole('dialog', { name: 'Import payment data' });
	await dialog.getByLabel('Bank CSV').setInputFiles({
		name: 'october-statement.csv',
		mimeType: 'text/csv',
		buffer: Buffer.from(
			'Date,Description,Amount\n24/10/2026,October salary,2500',
		),
	});
	await dialog.getByRole('button', { name: 'Import payments' }).click();

	await expect(
		page.getByText('1 payment imported from october-statement.csv.'),
	).toBeVisible();
});

test('should paginate payments and change the number of rows per page', async ({
	page,
}) => {
	const listRequests: URL[] = [];
	await page.route('**/api/payments/imports', (route) =>
		route.fulfill({ json: [paymentImport] }),
	);
	await page.route('**/api/payments?*', async (route) => {
		const url = new URL(route.request().url());
		listRequests.push(url);
		const requestedPage = Number(url.searchParams.get('page'));
		const requestedPageSize = Number(url.searchParams.get('pageSize'));
		await route.fulfill({
			json: {
				items: [payment],
				page: requestedPage,
				pageSize: requestedPageSize,
				totalItems: 51,
				totalPages: Math.ceil(51 / requestedPageSize),
			},
		});
	});

	await page.goto('/payments');
	const pagination = page.getByRole('navigation', {
		name: 'Payment pagination',
	});
	await expect(pagination.getByText('Page 1 of 3')).toBeVisible();
	await pagination.getByRole('button', { name: 'Next' }).click();
	await expect(pagination.getByText('Page 2 of 3')).toBeVisible();

	await pagination.getByRole('combobox', { name: 'Rows per page' }).click();
	await page.getByRole('option', { name: '10', exact: true }).click();

	await expect(pagination.getByText('Page 1 of 6')).toBeVisible();
	await expect
		.poll(() => listRequests.at(-1)?.searchParams.get('page'))
		.toBe('1');
	expect(listRequests.at(-1)?.searchParams.get('pageSize')).toBe('10');
});

test('should delete payments by file, date, or entirely', async ({ page }) => {
	const deleteRequests: string[] = [];
	await routePaymentData(page);
	await page.route('**/api/payments?*', async (route) => {
		if (route.request().method() === 'DELETE') {
			deleteRequests.push(route.request().url());
			await route.fulfill({ status: 204 });
			return;
		}
		await route.fulfill({
			json: {
				items: [payment],
				page: 1,
				pageSize: 25,
				totalItems: 1,
				totalPages: 1,
			},
		});
	});

	await page.goto('/payments');
	const deleteButton = page.getByRole('button', { name: 'Delete payments' });
	await expect(deleteButton).toBeEnabled();

	await deleteButton.click();
	let dialog = page.getByRole('dialog', { name: 'Delete imported payments' });
	await dialog.getByRole('combobox', { name: 'Imported file' }).click();
	await page.getByRole('option', { name: /october-statement.csv/ }).click();
	await dialog.getByRole('button', { name: 'Delete payments' }).click();
	await expect(dialog).toBeHidden();

	await deleteButton.click();
	dialog = page.getByRole('dialog', { name: 'Delete imported payments' });
	await dialog.getByRole('combobox', { name: 'Delete by' }).click();
	await page.getByRole('option', { name: 'Transaction date' }).click();
	await dialog.getByLabel('From date').fill('2026-10-01');
	await dialog.getByLabel('To date').fill('2026-10-31');
	await dialog.getByRole('button', { name: 'Delete payments' }).click();
	await expect(dialog).toBeHidden();

	await deleteButton.click();
	dialog = page.getByRole('dialog', { name: 'Delete imported payments' });
	await dialog.getByRole('combobox', { name: 'Delete by' }).click();
	await page.getByRole('option', { name: 'All payments' }).click();
	await dialog.getByRole('button', { name: 'Delete payments' }).click();

	await expect.poll(() => deleteRequests.length).toBe(3);
	expect(deleteRequests[0]).toContain(`importId=${importId}`);
	expect(deleteRequests[1]).toContain('dateAfter=2026-10-01');
	expect(deleteRequests[1]).toContain('dateBefore=2026-10-31');
	expect(deleteRequests[2]).toContain('all=true');
});
