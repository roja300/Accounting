import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { ThemeProvider, useTheme } from './theme-provider';

let systemPrefersDark = false;
let systemThemeListener: (() => void) | undefined;

function ThemeControls() {
	const { theme, setTheme } = useTheme();

	return (
		<>
			<output data-testid="theme">{theme}</output>
			<button
				type="button"
				onClick={() => setTheme('dark')}
			>
				Use dark theme
			</button>
			<input aria-label="Editable field" />
		</>
	);
}

function renderThemeProvider(
	defaultTheme: 'dark' | 'light' | 'system' = 'light',
) {
	return render(
		<ThemeProvider
			defaultTheme={defaultTheme}
			disableTransitionOnChange={false}
			storageKey="test-theme"
		>
			<ThemeControls />
		</ThemeProvider>,
	);
}

describe('ThemeProvider', () => {
	beforeEach(() => {
		systemPrefersDark = false;
		systemThemeListener = undefined;

		Object.defineProperty(window, 'matchMedia', {
			configurable: true,
			value: vi.fn().mockImplementation((query: string) => ({
				get matches() {
					return systemPrefersDark;
				},
				media: query,
				onchange: null,
				addListener: vi.fn(),
				removeListener: vi.fn(),
				addEventListener: vi.fn(
					(event: string, listener: () => void) => {
						if (event === 'change') {
							systemThemeListener = listener;
						}
					},
				),
				removeEventListener: vi.fn(),
				dispatchEvent: vi.fn(),
			})),
		});
	});

	it('uses a valid stored theme and applies it to the document', async () => {
		localStorage.setItem('test-theme', 'dark');

		renderThemeProvider('light');

		expect(screen.getByTestId('theme')).toHaveTextContent('dark');
		await waitFor(() =>
			expect(document.documentElement).toHaveClass('dark'),
		);
	});

	it('updates and persists a selected theme', async () => {
		const user = userEvent.setup();
		renderThemeProvider();

		await user.click(
			screen.getByRole('button', { name: 'Use dark theme' }),
		);

		expect(screen.getByTestId('theme')).toHaveTextContent('dark');
		expect(localStorage.getItem('test-theme')).toBe('dark');
		await waitFor(() =>
			expect(document.documentElement).toHaveClass('dark'),
		);
	});

	it('toggles with the keyboard shortcut outside editable controls', () => {
		renderThemeProvider();

		fireEvent.keyDown(window, { key: 'd' });
		expect(screen.getByTestId('theme')).toHaveTextContent('dark');

		fireEvent.keyDown(
			screen.getByRole('textbox', { name: 'Editable field' }),
			{
				key: 'd',
			},
		);
		expect(screen.getByTestId('theme')).toHaveTextContent('dark');

		fireEvent.keyDown(window, { key: 'd', ctrlKey: true });
		expect(screen.getByTestId('theme')).toHaveTextContent('dark');
	});

	it('tracks system colour-scheme changes', async () => {
		renderThemeProvider('system');

		await waitFor(() =>
			expect(document.documentElement).toHaveClass('light'),
		);

		systemPrefersDark = true;
		fireEvent(window, new Event('change'));
		systemThemeListener?.();

		await waitFor(() =>
			expect(document.documentElement).toHaveClass('dark'),
		);
	});
});
