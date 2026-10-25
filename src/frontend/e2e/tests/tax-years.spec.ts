import { expect, type Page, test } from '@playwright/test';

const configuration = {
	employmentIncome: 0,
	employmentTaxPaid: 0,
	pensionContributions: 0,
	allowableExpenses: 0,
	additionalAllowances: 0,
	personalAllowance: 12570,
	personalAllowanceTaperThreshold: 100000,
	basicRateLimit: 37700,
	higherRateLimit: 125140,
	basicRate: 0.2,
	higherRate: 0.4,
	additionalRate: 0.45,
	nationalInsuranceLowerLimit: 12570,
	nationalInsuranceUpperLimit: 50270,
	nationalInsuranceMainRate: 0.06,
	nationalInsuranceAdditionalRate: 0.02,
};

const taxYear = {
	startYear: 2026,
	startsOn: '2026-04-06',
	endsOn: '2027-04-05',
	status: 'draft',
	expectedPay: 52000,
	totalEarned: 40000,
	estimatedIncomeTax: 5486,
	estimatedNationalInsurance: 1645.8,
	amountToPay: 7131.8,
	configuration,
} as const;

async function routeTaxYears(
	page: Page,
	taxYears: ReadonlyArray<typeof taxYear> = [taxYear],
) {
	await page.route('**/api/tax-years', (route) =>
		route.fulfill({ json: taxYears }),
	);
}

test('should display ongoing tax-year figures as cards', async ({ page }) => {
	await routeTaxYears(page);

	await page.goto('/tax');

	await expect(
		page.getByRole('heading', { name: 'Tax years' }),
	).toBeVisible();
	await expect(
		page.getByRole('heading', { name: '2026/27 tax year' }),
	).toBeVisible();
	await expect(page.getByText('Expected pay').locator('..')).toContainText(
		'£52,000.00',
	);
	await expect(page.getByText('Total earned').locator('..')).toContainText(
		'£40,000.00',
	);
	await expect(
		page.getByText('Estimated amount to pay').locator('..'),
	).toContainText('£7,131.80');
	await expect(page.getByText('VAT is not included')).toBeVisible();
});

test('should configure employment and pension details for a draft tax year', async ({
	page,
}) => {
	let savedBody: Record<string, number> | undefined;
	await routeTaxYears(page);
	await page.route('**/api/tax-years/2026', async (route) => {
		savedBody = route.request().postDataJSON() as Record<string, number>;
		await route.fulfill({
			json: {
				...taxYear,
				status: 'configured',
				amountToPay: 6500,
				configuration: savedBody,
			},
		});
	});

	await page.goto('/tax');
	await page.getByRole('button', { name: 'Configure tax year' }).click();
	const dialog = page.getByRole('dialog', { name: 'Configure 2026/27' });
	await dialog.getByLabel('Employment income').fill('18000');
	await dialog.getByLabel('Tax already paid through employment').fill('1200');
	await dialog.getByLabel('Pension contributions').fill('2500');
	await dialog.getByRole('button', { name: 'Save configuration' }).click();

	await expect(dialog).toBeHidden();
	await expect(page.getByText('configured', { exact: true })).toBeVisible();
	expect(savedBody?.employmentIncome).toBe(18000);
	expect(savedBody?.employmentTaxPaid).toBe(1200);
	expect(savedBody?.pensionContributions).toBe(2500);
});

test('should scroll tax-year cards without scrolling the page', async ({
	page,
}) => {
	await page.setViewportSize({ width: 1200, height: 800 });
	await routeTaxYears(
		page,
		Array.from({ length: 12 }, (_, index) => ({
			...taxYear,
			startYear: taxYear.startYear - index,
		})),
	);

	await page.goto('/tax');
	const cardScroll = page.getByTestId('tax-year-card-scroll');
	await expect(cardScroll).toBeVisible();
	const metrics = await cardScroll.evaluate((element) => ({
		bottom: element.getBoundingClientRect().bottom,
		clientHeight: element.clientHeight,
		documentHeight: document.documentElement.scrollHeight,
		scrollHeight: element.scrollHeight,
		viewportHeight: window.innerHeight,
	}));

	expect(metrics.bottom).toBeLessThanOrEqual(metrics.viewportHeight);
	expect(metrics.scrollHeight).toBeGreaterThan(metrics.clientHeight);
	expect(metrics.documentHeight).toBeLessThanOrEqual(metrics.viewportHeight);
});
