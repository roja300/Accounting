import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, expect, test, vi } from 'vitest';

import { NewWorkPage } from '@/features/invoices/new-work-page';

afterEach(() => {
	vi.unstubAllGlobals();
});

test('should show validation messages and not submit an empty work record', async () => {
	const user = userEvent.setup();
	const fetchMock = mockInvoiceApi();
	renderPage();

	await user.click(screen.getByRole('button', { name: 'Save work' }));

	expect(screen.getByText('Enter a description.')).toBeInTheDocument();
	expect(screen.getByText('Select a currency.')).toBeInTheDocument();
	expect(screen.getByText('Select or enter a client.')).toBeInTheDocument();
	expect(screen.getByText('Select or enter a project.')).toBeInTheDocument();
	expect(screen.getByText('Enter a value.')).toBeInTheDocument();
	expect(fetchMock).not.toHaveBeenCalledWith(
		'/api/invoices',
		expect.objectContaining({ method: 'POST' }),
	);
});

test('should submit the typed dto and navigate after confirming success', async () => {
	const user = userEvent.setup();
	const fetchMock = mockInvoiceApi();
	renderPage();

	await user.type(
		screen.getByRole('textbox', { name: 'Description' }),
		'Monthly bookkeeping',
	);
	await user.type(
		screen.getByRole('spinbutton', { name: 'Value' }),
		'125.50',
	);
	await user.click(screen.getByRole('combobox', { name: 'Currency' }));
	await user.click(
		screen.getByRole('option', { name: 'Pound sterling (GBP)' }),
	);
	await user.type(
		screen.getByRole('combobox', { name: 'Client' }),
		'New Client',
	);
	expect(
		screen.getByText('A new client named “New Client” will be created.'),
	).toBeInTheDocument();
	await user.keyboard('{Escape}');
	await user.type(
		screen.getByRole('combobox', { name: 'Project' }),
		'Retainer',
	);
	await user.keyboard('{Escape}');
	await user.click(screen.getByRole('button', { name: 'Save work' }));

	expect(
		await screen.findByRole('heading', { name: 'Work record saved' }),
	).toBeInTheDocument();
	const postCall = fetchMock.mock.calls.find(
		([input, init]) => input === '/api/invoices' && init?.method === 'POST',
	);
	expect(postCall).toBeDefined();
	expect(JSON.parse(String(postCall?.[1]?.body))).toEqual({
		description: 'Monthly bookkeeping',
		value: 125.5,
		currency: 'GBP',
		client: 'New Client',
		project: 'Retainer',
	});

	await user.click(
		screen.getByRole('button', { name: 'Return to invoices' }),
	);
	expect(screen.getByText('Invoices destination')).toBeInTheDocument();
});

function renderPage() {
	return render(
		<MemoryRouter initialEntries={['/work/new']}>
			<Routes>
				<Route
					path="/work/new"
					element={<NewWorkPage />}
				/>
				<Route
					path="/invoices"
					element={<p>Invoices destination</p>}
				/>
			</Routes>
		</MemoryRouter>,
	);
}

function mockInvoiceApi() {
	const fetchMock = vi.fn(
		(input: RequestInfo | URL, init?: RequestInit): Promise<Response> => {
			if (String(input) === '/api/invoices/options') {
				return Promise.resolve(
					jsonResponse({
						clients: ['Acme Ltd'],
						projects: ['Website refresh'],
					}),
				);
			}

			if (String(input) === '/api/invoices' && init?.method === 'POST') {
				const request = JSON.parse(String(init.body));
				return Promise.resolve(
					jsonResponse({
						id: '1f184969-9587-4103-b6ee-80a467669fa8',
						...request,
						createdAt: '2026-10-18T12:30:00Z',
					}),
				);
			}

			throw new Error(`Unexpected request to ${String(input)}.`);
		},
	);
	vi.stubGlobal('fetch', fetchMock);
	return fetchMock;
}

function jsonResponse(body: unknown): Response {
	return {
		ok: true,
		status: 200,
		json: async () => body,
	} as Response;
}
