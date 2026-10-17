import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';

import './index.css';
import { ThemeProvider } from '@/components/theme-provider.tsx';
import App from './App.tsx';

const rootElement = document.getElementById('root');

if (!rootElement) {
	throw new Error('Root element was not found');
}

createRoot(rootElement).render(
	<StrictMode>
		<BrowserRouter>
			<ThemeProvider>
				<App />
			</ThemeProvider>
		</BrowserRouter>
	</StrictMode>,
);
