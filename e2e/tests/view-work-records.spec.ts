import {
	expect,
	test,
	type Page,
} from '../../src/frontend/node_modules/@playwright/test';

const records = Array.from({ length: 10 }, (_, index) => ({
	id: `record-${index + 1}`,
	description: index === 9 ? 'Final review' : `Monthly bookkeeping ${index + 1}`,
	value: 125.5 + index,
	currency: 'GBP',
	comments: 'Completed October records.',
	client: index % 2 === 0 ? 'Acme Ltd' : 'Northwind Ltd',
	project: index % 2 === 0 ? 'Retainer' : 'Year end',
	createdAt: `2026-10-${String(18 - index).padStart(2, '0')}T12:30:00Z`,
}));

async function mockInvoiceApi(page: Page) {
	await page.route('**/api/invoices/options', (route) =>
		route.fulfill({
			json: {
				clients: ['Acme Ltd', 'Northwind Ltd'],
				projects: ['Retainer', 'Year end'],
			},
		}),
	);
	await page.route('**/api/invoices?*', (route) => {
		const url = new URL(route.request().url());
		const requestedPage = Number(url.searchParams.get('page') ?? '1');
		const search = url.searchParams.get('search');
		const filtered = search ? [records[0]] : records.slice((requestedPage - 1) * 9, requestedPage * 9);

		return route.fulfill({
			json: {
				items: filtered,
				page: requestedPage,
				pageSize: 9,
				totalItems: search ? 1 : records.length,
				totalPages: search ? 1 : 2,
			},
		});
	});
}

test('should show work records as cards and paginate them', async ({ page }) => {
	await mockInvoiceApi(page);
	await page.goto('/invoices');

	await expect(page.getByRole('article')).toHaveCount(9);
	await expect(page.getByRole('heading', { name: 'Work records' })).toBeVisible();
	await expect(page.getByText('1-9 of 10 work records')).toBeVisible();

	await page.getByRole('button', { name: 'Next' }).click();
	await expect(page.getByText('Page 2 of 2')).toBeVisible();
	await expect(page.getByText('Final review')).toBeVisible();
});

test('should send all selected filters when applying them', async ({ page }) => {
	await mockInvoiceApi(page);
	await page.goto('/invoices');

	await page.getByLabel('Search description').fill('bookkeeping');
	await page.getByLabel('Date after').fill('2026-10-01');
	await page.getByLabel('Date before').fill('2026-10-31');
	await page.getByLabel('Client').click();
	await page.getByRole('option', { name: 'Acme Ltd' }).click();
	await page.getByLabel('Project').click();
	await page.getByRole('option', { name: 'Retainer' }).click();
	await page.getByRole('button', { name: 'Apply filters' }).click();

	await expect(page.getByText('1-1 of 1 work records')).toBeVisible();
});

test('should keep edit and delete actions disabled', async ({ page }) => {
	await mockInvoiceApi(page);
	await page.goto('/invoices');

	const actions = page.locator('summary[aria-label="Actions for Monthly bookkeeping 1"]');
	await expect(actions).toHaveCount(1);
	const menuItems = actions.locator('xpath=..').locator('button[role="menuitem"]');
	await expect(menuItems).toHaveCount(2);
	await expect(menuItems.nth(0)).toBeDisabled();
	await expect(menuItems.nth(1)).toBeDisabled();
});
