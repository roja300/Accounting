import { expect, test } from '@playwright/test';

test('should configure business information and create a customer', async ({
	page,
}) => {
	let savedBusiness: Record<string, unknown> | undefined;
	let savedCustomer: Record<string, unknown> | undefined;

	await page.route('**/api/profile/business', async (route) => {
		if (route.request().method() === 'GET') {
			await route.fulfill({
				json: {
					businessType: 'soleTrader',
					legalName: '',
					tradingName: '',
					email: '',
					phone: '',
					addressLine1: '',
					addressLine2: '',
					city: '',
					county: '',
					postcode: '',
					companyNumber: '',
					vatNumber: '',
					updatedAt: '2026-10-19T12:00:00Z',
				},
			});
			return;
		}

		savedBusiness = route.request().postDataJSON() as Record<
			string,
			unknown
		>;
		await route.fulfill({
			json: { ...savedBusiness, updatedAt: '2026-10-19T12:00:00Z' },
		});
	});
	await page.route('**/api/profile/clients', async (route) => {
		if (route.request().method() === 'GET') {
			await route.fulfill({ json: [] });
			return;
		}

		savedCustomer = route.request().postDataJSON() as Record<
			string,
			unknown
		>;
		await route.fulfill({
			status: 201,
			json: {
				id: 'b31f5928-64b0-4dd7-aad1-3a3ca55158ed',
				...savedCustomer,
				createdAt: '2026-10-19T12:00:00Z',
				updatedAt: '2026-10-19T12:00:00Z',
			},
		});
	});
	await page.route('**/api/profile/invoice-configuration', (route) =>
		route.fulfill({
			json: {
				style: 'basic',
				hasLogo: false,
				logoFileName: '',
				updatedAt: null,
			},
		}),
	);

	await page.goto('/profile');
	await expect(page.getByRole('heading', { name: 'Profile' })).toBeVisible();
	await expect(page.getByLabel('Legal name')).toHaveAccessibleDescription(
		'Your full personal name or registered company name.',
	);
	const businessType = page.getByRole('combobox', { name: 'Business type' });
	await businessType.click();
	await page.getByRole('option', { name: 'Limited company' }).click();
	await expect(businessType).toContainText('Limited company');
	await businessType.click();
	await page.getByRole('option', { name: 'Sole trader' }).click();
	await page.getByLabel('Legal name').fill('Example Freelancer');
	await page.getByLabel('Email').first().fill('owner@example.test');
	await page.getByLabel('Address line 1').first().fill('1 High Street');
	await page.getByLabel('Town or city').first().fill('London');
	await page.getByLabel('Postcode').first().fill('SW1A 1AA');
	await page.getByRole('button', { name: 'Save business' }).click();
	await expect(page.getByText('Business information saved.')).toBeVisible();
	expect(savedBusiness?.legalName).toBe('Example Freelancer');

	await page.getByRole('button', { name: /Clients/ }).click();
	const profileBottom = await page
		.locator('section[aria-labelledby="profile-title"]')
		.evaluate((section) => section.getBoundingClientRect().bottom);
	const viewportHeight = await page.evaluate(() => window.innerHeight);
	expect(profileBottom).toBeGreaterThanOrEqual(viewportHeight - 1);
	await page.getByRole('button', { name: 'Add customer' }).click();
	const customerDialog = page.getByRole('dialog', { name: 'Add customer' });
	await expect(
		customerDialog.getByLabel('Customer name'),
	).toHaveAccessibleDescription(
		'Person or organisation that will receive the invoice.',
	);
	await customerDialog.getByLabel('Customer name').fill('Acme Ltd');
	await customerDialog.getByLabel('Email').fill('accounts@acme.test');
	await customerDialog.getByLabel('Address line 1').fill('2 Market Road');
	await customerDialog.getByLabel('Town or city').fill('Manchester');
	await customerDialog.getByLabel('Postcode').fill('M1 1AA');
	await customerDialog.getByRole('button', { name: 'Save customer' }).click();

	await expect(
		page.getByRole('cell', { name: 'Acme Ltd', exact: true }),
	).toBeVisible();
	expect(savedCustomer?.email).toBe('accounts@acme.test');
});

test('should configure an invoice style and upload a logo', async ({
	page,
}) => {
	let savedStyle: string | undefined;
	let uploadedLogo = false;
	await page.route('**/api/profile/business', (route) =>
		route.fulfill({ status: 404 }),
	);
	await page.route('**/api/profile/clients', (route) =>
		route.fulfill({ json: [] }),
	);
	await page.route('**/api/profile/invoice-configuration', async (route) => {
		if (route.request().method() === 'GET') {
			await route.fulfill({
				json: {
					style: 'basic',
					hasLogo: false,
					logoFileName: '',
					updatedAt: null,
				},
			});
			return;
		}
		savedStyle = (route.request().postDataJSON() as { style: string })
			.style;
		await route.fulfill({
			json: {
				style: savedStyle,
				hasLogo: false,
				logoFileName: '',
				updatedAt: '2026-10-24T12:00:00Z',
			},
		});
	});
	await page.route(
		'**/api/profile/invoice-configuration/logo',
		async (route) => {
			uploadedLogo = route.request().method() === 'PUT';
			await route.fulfill({
				json: {
					style: savedStyle ?? 'modern',
					hasLogo: true,
					logoFileName: 'logo.png',
					updatedAt: '2026-10-24T12:00:00Z',
				},
			});
		},
	);

	await page.goto('/profile');
	await page.getByRole('button', { name: /Invoice configuration/ }).click();
	const demoLinks = page.getByRole('link', { name: 'View demo PDF' });
	await expect(demoLinks).toHaveCount(3);
	await expect(demoLinks.nth(0)).toHaveAttribute(
		'href',
		'/api/profile/invoice-configuration/demo/basic',
	);
	await expect(demoLinks.nth(1)).toHaveAttribute(
		'href',
		'/api/profile/invoice-configuration/demo/modern',
	);
	await expect(demoLinks.nth(2)).toHaveAttribute(
		'href',
		'/api/profile/invoice-configuration/demo/minimal',
	);
	await page.getByText('Modern', { exact: true }).click();
	await page.getByLabel('Upload logo').setInputFiles({
		name: 'logo.png',
		mimeType: 'image/png',
		buffer: Buffer.from('iVBORw0KGgo=', 'base64'),
	});
	await page.getByRole('button', { name: 'Save configuration' }).click();

	await expect(page.getByText('Invoice configuration saved.')).toBeVisible();
	expect(savedStyle).toBe('modern');
	expect(uploadedLogo).toBe(true);
});

test('should prevent recording work until a customer exists', async ({
	page,
}) => {
	await page.route('**/api/invoices/options', (route) =>
		route.fulfill({ json: { clients: [], projects: [] } }),
	);

	await page.goto('/work/new');
	await expect(
		page.getByText('Add a customer before recording work'),
	).toBeVisible();
	await expect(
		page.getByRole('button', { name: 'Save work' }),
	).toBeDisabled();
	await expect(
		page.getByRole('button', { name: 'Open business profile' }),
	).toHaveAttribute('href', '/profile');
});
