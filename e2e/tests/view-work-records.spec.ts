import {
	expect,
	type Page,
	test,
} from '../../src/frontend/node_modules/@playwright/test';

const records = Array.from({ length: 10 }, (_, index) => ({
	id: `record-${index + 1}`,
	description:
		index === 9 ? 'Final review' : `Monthly bookkeeping ${index + 1}`,
	value: 125.5 + index,
	currency: 'GBP',
	comments: 'Completed October records.',
	client: index % 2 === 0 ? 'Acme Ltd' : 'Northwind Ltd',
	project: index % 2 === 0 ? 'Retainer' : 'Year end',
	createdAt: `2026-10-${String(18 - index).padStart(2, '0')}T12:30:00Z`,
}));

async function mockInvoiceApi(page: Page) {
	const deletedRecordIds = new Set<string>();
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
		const visibleRecords = records.filter(
			(record) => !deletedRecordIds.has(record.id),
		);
		const filtered = search
			? visibleRecords
					.filter((record) =>
						record.description
							.toLowerCase()
							.includes(search.toLowerCase()),
					)
					.slice(0, 1)
			: visibleRecords.slice((requestedPage - 1) * 9, requestedPage * 9);

		return route.fulfill({
			json: {
				items: filtered,
				page: requestedPage,
				pageSize: 9,
				totalItems: search ? filtered.length : visibleRecords.length,
				totalPages: search ? 1 : Math.ceil(visibleRecords.length / 9),
			},
		});
	});
	await page.route('**/api/invoices/*', (route) => {
		if (route.request().method() !== 'DELETE') return route.fallback();
		deletedRecordIds.add(route.request().url().split('/').at(-1) ?? '');
		return route.fulfill({ status: 204 });
	});
}

test('should show work records as cards and paginate them', async ({
	page,
}) => {
	await mockInvoiceApi(page);
	await page.goto('/invoices');

	await expect(page.getByRole('article')).toHaveCount(9);
	await expect(
		page.getByRole('heading', { name: 'Work records' }),
	).toBeVisible();
	await expect(page.getByText('1-9 of 10 work records')).toBeVisible();

	await page.getByRole('button', { name: 'Next' }).click();
	await expect(page.getByText('Page 2 of 2')).toBeVisible();
	await expect(page.getByText('Final review')).toBeVisible();
});

test('should send all selected filters when applying them', async ({
	page,
}) => {
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

test('should delete a work record after confirmation', async ({ page }) => {
	await mockInvoiceApi(page);
	await page.goto('/invoices');

	const actions = page.locator(
		'summary[aria-label="Actions for Monthly bookkeeping 1"]',
	);
	await expect(actions).toHaveCount(1);
	await actions.click();
	const menuItems = actions.locator('xpath=..').getByRole('menuitem');
	await expect(menuItems).toHaveCount(2);
	await expect(menuItems.nth(0)).toHaveAttribute(
		'href',
		'/work/record-1/edit',
	);
	await menuItems.nth(1).click();
	await expect(
		page.getByRole('heading', { name: 'Delete work record?' }),
	).toBeVisible();
	await expect(
		page.getByText(
			'“Monthly bookkeeping 1” will be hidden from your work records.',
		),
	).toBeVisible();

	const deleteRequest = page.waitForRequest(
		(request) =>
			request.method() === 'DELETE' &&
			request.url().endsWith('/api/invoices/record-1'),
	);
	await page.getByRole('button', { name: 'Delete work record' }).click();
	await deleteRequest;

	await expect(
		page.getByText('“Monthly bookkeeping 1” was deleted.'),
	).toBeVisible();
	await expect(page.getByRole('article')).toHaveCount(9);
	await expect(page.getByText('1-9 of 9 work records')).toBeVisible();
	await expect(
		page.getByText('Monthly bookkeeping 1', { exact: true }),
	).toHaveCount(0);
});
