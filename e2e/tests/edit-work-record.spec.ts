import {
	expect,
	type Page,
	test,
} from '../../src/frontend/node_modules/@playwright/test';

const record = {
	id: '1f184969-9587-4103-b6ee-80a467669fa8',
	description: 'Monthly bookkeeping',
	value: 125.5,
	currency: 'GBP',
	comments: 'Completed October records.',
	client: 'Acme Ltd',
	project: 'Retainer',
	createdAt: '2026-10-18T12:30:00Z',
};

async function mockInvoiceApi(page: Page) {
	let updateBody: Record<string, unknown> | undefined;

	await page.route('**/api/invoices/options', (route) =>
		route.fulfill({
			json: {
				clients: ['Acme Ltd', 'Northwind Ltd'],
				projects: ['Retainer', 'Year end'],
			},
		}),
	);
	await page.route(`**/api/invoices/${record.id}`, async (route) => {
		if (route.request().method() === 'PUT') {
			updateBody = route.request().postDataJSON() as Record<
				string,
				unknown
			>;
			await route.fulfill({ json: { ...record, ...updateBody } });
			return;
		}

		await route.fulfill({ json: record });
	});
	await page.route('**/api/invoices?*', (route) =>
		route.fulfill({
			json: {
				items: [record],
				page: 1,
				pageSize: 9,
				totalItems: 1,
				totalPages: 1,
			},
		}),
	);

	return () => updateBody;
}

test('should prepopulate and update a work record from its card action', async ({
	page,
}) => {
	const getUpdateBody = await mockInvoiceApi(page);
	await page.goto('/invoices');

	await page
		.locator('summary[aria-label="Actions for Monthly bookkeeping"]')
		.click();
	await page.getByRole('menuitem', { name: 'Edit' }).click();

	await expect(page).toHaveURL(`/work/${record.id}/edit`);
	await expect(
		page.getByRole('heading', { name: 'Edit work' }),
	).toBeVisible();
	await expect(
		page.getByRole('textbox', { name: 'Description' }),
	).toHaveValue(record.description);
	await expect(page.getByRole('spinbutton', { name: 'Value' })).toHaveValue(
		'125.5',
	);
	await expect(
		page.getByRole('combobox', { name: 'Currency' }),
	).toContainText('Pound sterling (GBP)');
	await expect(page.getByRole('combobox', { name: 'Client' })).toHaveValue(
		record.client,
	);
	await expect(page.getByRole('combobox', { name: 'Project' })).toHaveValue(
		record.project,
	);
	await expect(
		page.getByRole('textbox', { name: /Additional comments/ }),
	).toHaveValue(record.comments);

	await page
		.getByRole('textbox', { name: 'Description' })
		.fill('Year-end accounts');
	await page
		.getByRole('textbox', { name: /Additional comments/ })
		.fill('Draft prepared.');
	await page.getByRole('button', { name: 'Save changes' }).click();

	await expect(
		page.getByRole('heading', { name: 'Work record updated' }),
	).toBeVisible();
	expect(getUpdateBody()).toEqual({
		description: 'Year-end accounts',
		value: 125.5,
		currency: 'GBP',
		comments: 'Draft prepared.',
		client: 'Acme Ltd',
		project: 'Retainer',
	});
});

test('should apply existing validation before updating a work record', async ({
	page,
}) => {
	const getUpdateBody = await mockInvoiceApi(page);
	await page.goto(`/work/${record.id}/edit`);

	await page.getByRole('textbox', { name: 'Description' }).fill('');
	await page.getByRole('spinbutton', { name: 'Value' }).fill('');
	await page.getByRole('button', { name: 'Save changes' }).click();

	await expect(page.getByText('Enter a description.')).toBeVisible();
	await expect(page.getByText('Enter a value.')).toBeVisible();
	expect(getUpdateBody()).toBeUndefined();
});
