import { render, screen } from '@testing-library/react';
import { expect, test } from 'vitest';

import { NewWorkPage } from '@/features/invoices/new-work-page';

test('should present the new work details in a draft card', () => {
	render(<NewWorkPage />);

	expect(
		screen.getByRole('heading', { name: 'New work' }),
	).toBeInTheDocument();
	expect(
		screen.getByRole('region', { name: 'Work details' }),
	).toBeInTheDocument();
	expect(screen.getByText('Draft')).toBeInTheDocument();
	expect(screen.getByText('Client')).toBeInTheDocument();
	expect(screen.getByText('Work date')).toBeInTheDocument();
	expect(screen.getByText('Amount')).toBeInTheDocument();
});
