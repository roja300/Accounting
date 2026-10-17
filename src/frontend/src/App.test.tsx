import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';

import { App } from './App';

describe('App', () => {
	it('renders the project welcome content', () => {
		render(<App />);

		expect(
			screen.getByRole('heading', { name: 'Project ready!' }),
		).toBeInTheDocument();
		expect(screen.getByRole('button', { name: 'Button' })).toBeEnabled();
		expect(screen.getByText('d', { selector: 'kbd' })).toBeInTheDocument();
	});
});
