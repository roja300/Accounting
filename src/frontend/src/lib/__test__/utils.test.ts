import { cn } from 'cn';
import { describe, expect, it } from 'vitest';

describe('cn', () => {
	it('ignores empty values and resolves Tailwind conflicts', () => {
		expect(cn('px-2', undefined, 'px-4', 'font-medium')).toBe(
			'px-4 font-medium',
		);
	});
});
