import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { Button } from './button';

describe('Button', () => {
	it('forwards interactions to the underlying button', async () => {
		const user = userEvent.setup();
		const handleClick = vi.fn();

		render(<Button onClick={handleClick}>Save</Button>);
		await user.click(screen.getByRole('button', { name: 'Save' }));

		expect(handleClick).toHaveBeenCalledOnce();
	});

	it('does not invoke interactions while disabled', async () => {
		const user = userEvent.setup();
		const handleClick = vi.fn();

		render(
			<Button
				disabled
				onClick={handleClick}
			>
				Save
			</Button>,
		);
		const button = screen.getByRole('button', { name: 'Save' });
		await user.click(button);

		expect(button).toBeDisabled();
		expect(handleClick).not.toHaveBeenCalled();
	});

	it('applies the requested variant, size, and custom classes', () => {
		render(
			<Button
				className="custom-class"
				size="lg"
				variant="destructive"
			>
				Delete
			</Button>,
		);

		expect(screen.getByRole('button', { name: 'Delete' })).toHaveClass(
			'custom-class',
			'h-9',
			'bg-destructive/10',
		);
	});
});
