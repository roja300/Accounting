import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { expect, test } from 'vitest';

import { App } from '@/App';

function renderApp(initialPath: string) {
	return render(
		<MemoryRouter initialEntries={[initialPath]}>
			<App />
		</MemoryRouter>,
	);
}

test('should render the Tax page with its navigation item active', () => {
	renderApp('/tax');

	expect(
		screen.getByRole('navigation', { name: 'Primary navigation' }),
	).toBeInTheDocument();
	expect(
		screen.getByRole('region', { name: 'Tax page' }),
	).toBeInTheDocument();
	expect(screen.getByRole('link', { name: 'Tax' })).toHaveAttribute(
		'aria-current',
		'page',
	);
});

test('should navigate from Tax to exported invoices', async () => {
	const user = userEvent.setup();
	renderApp('/tax');

	await user.click(screen.getByRole('link', { name: 'Invoices' }));

	expect(
		screen.getByRole('region', { name: 'Exported invoices page' }),
	).toBeInTheDocument();
	expect(screen.getByRole('link', { name: 'Invoices' })).toHaveAttribute(
		'aria-current',
		'page',
	);
});

test('should redirect an unknown route to Tax', () => {
	renderApp('/missing');

	expect(
		screen.getByRole('region', { name: 'Tax page' }),
	).toBeInTheDocument();
});
