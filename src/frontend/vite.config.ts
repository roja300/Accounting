import { resolve } from 'node:path';
import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// https://vite.dev/config/
export default defineConfig({
	plugins: [react(), tailwindcss()],
	resolve: {
		alias: {
			'@': resolve(import.meta.dirname, './src'),
		},
	},
	server: {
		watch: {
			usePolling: true,
			interval: 200,
		},
		proxy: {
			'/api': {
				target: process.env.VITE_API_PROXY_TARGET ?? 'http://localhost:8080',
				changeOrigin: true,
				rewrite: (path) => path.replace(/^\/api/, ''),
			},
		},
	},
});
