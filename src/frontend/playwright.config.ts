import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
	testDir: '../../e2e/tests',
	use: {
		// biome-ignore lint/style/useNamingConvention: Playwright requires this option name.
		baseURL: 'http://127.0.0.1:5173',
		trace: 'on-first-retry',
	},
	webServer: {
		command: 'npm run dev -- --host 127.0.0.1',
		url: 'http://127.0.0.1:5173',
		reuseExistingServer: !process.env.CI,
	},
	projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
