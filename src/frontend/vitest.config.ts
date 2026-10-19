import { resolve } from 'node:path';
import react from '@vitejs/plugin-react';
import { configDefaults, defineConfig } from 'vitest/config';

export default defineConfig({
	plugins: [react()],
	resolve: {
		alias: {
			'@': resolve(import.meta.dirname, './src'),
		},
	},
	test: {
		environment: 'jsdom',
		exclude: [...configDefaults.exclude, 'e2e/**'],
		setupFiles: './src/testing/setup.ts',
	},
});
