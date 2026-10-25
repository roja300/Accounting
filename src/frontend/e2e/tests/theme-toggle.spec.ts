import { expect, test } from '@playwright/test';

test('should toggle the colour theme from the top bar', async ({ page }) => {
	await page.addInitScript(() => localStorage.setItem('theme', 'light'));
	await page.goto('/tax');

	const themeToggle = page.getByRole('button', {
		name: 'Toggle colour theme',
	});
	await expect(page.locator('html')).toHaveClass(/light/);

	await themeToggle.hover();
	await expect(
		page.getByRole('tooltip', {
			name: 'Toggle light or dark mode. Press D anywhere.',
		}),
	).toBeVisible();

	await themeToggle.click();
	await expect(page.locator('html')).toHaveClass(/dark/);
	await expect
		.poll(() => page.evaluate(() => localStorage.getItem('theme')))
		.toBe('dark');

	await page.keyboard.press('d');
	await expect(page.locator('html')).toHaveClass(/light/);
});
