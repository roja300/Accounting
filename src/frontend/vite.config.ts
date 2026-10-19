import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

function getHttpsOptions() {
	const certificatePath = process.env.HTTPS_CERTIFICATE_PATH;
	const keyPath = process.env.HTTPS_KEY_PATH;

	if (!(certificatePath || keyPath)) return undefined;
	if (!(certificatePath && keyPath)) {
		throw new Error(
			'HTTPS_CERTIFICATE_PATH and HTTPS_KEY_PATH must be provided together.',
		);
	}

	return {
		cert: readFileSync(certificatePath),
		key: readFileSync(keyPath),
	};
}

// https://vite.dev/config/
export default defineConfig({
	plugins: [react(), tailwindcss()],
	resolve: {
		alias: {
			'@': resolve(import.meta.dirname, './src'),
		},
	},
	server: {
		https: getHttpsOptions(),
		watch: {
			usePolling: true,
			interval: 200,
		},
		proxy: {
			'/api': {
				target:
					process.env.VITE_API_PROXY_TARGET ??
					'http://localhost:8080',
				changeOrigin: true,
				secure: process.env.API_PROXY_VERIFY_TLS !== 'false',
				rewrite: (path) => path.replace(/^\/api/, ''),
			},
		},
	},
});
