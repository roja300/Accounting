import { z } from 'zod';

export const DESCRIPTION_MAX_LENGTH = 120;
export const COMMENTS_MAX_LENGTH = 500;
export const RELATED_RECORD_MAX_LENGTH = 80;

export const currencyOptions = [
	{ label: 'Pound sterling (GBP)', value: 'GBP' },
	{ label: 'Euro (EUR)', value: 'EUR' },
	{ label: 'US dollar (USD)', value: 'USD' },
] as const;

export const createWorkRecordSchema = z.object({
	description: z
		.string()
		.trim()
		.min(1, 'Enter a description.')
		.max(
			DESCRIPTION_MAX_LENGTH,
			`Description must be ${DESCRIPTION_MAX_LENGTH} characters or fewer.`,
		),
	value: z
		.number({
			// biome-ignore lint/style/useNamingConvention: Zod v3 defines these option keys.
			required_error: 'Enter a value.',
			// biome-ignore lint/style/useNamingConvention: Zod v3 defines these option keys.
			invalid_type_error: 'Enter a value.',
		})
		.positive('Value must be greater than zero.')
		.max(999_999_999.99, 'Value must be 999,999,999.99 or less.'),
	currency: z.enum(['GBP', 'EUR', 'USD'], {
		// biome-ignore lint/style/useNamingConvention: Zod v3 defines these option keys.
		required_error: 'Select a currency.',
		// biome-ignore lint/style/useNamingConvention: Zod v3 defines these option keys.
		invalid_type_error: 'Select a currency.',
	}),
	comments: z
		.string()
		.trim()
		.min(1, 'Enter additional comments.')
		.max(
			COMMENTS_MAX_LENGTH,
			`Comments must be ${COMMENTS_MAX_LENGTH} characters or fewer.`,
		),
	client: z
		.string()
		.trim()
		.min(1, 'Select or enter a client.')
		.max(
			RELATED_RECORD_MAX_LENGTH,
			`Client name must be ${RELATED_RECORD_MAX_LENGTH} characters or fewer.`,
		),
	project: z
		.string()
		.trim()
		.min(1, 'Select or enter a project.')
		.max(
			RELATED_RECORD_MAX_LENGTH,
			`Project name must be ${RELATED_RECORD_MAX_LENGTH} characters or fewer.`,
		),
});

export type CreateWorkRecordDto = z.output<typeof createWorkRecordSchema>;
export type CreateWorkRecordFormValues = z.input<typeof createWorkRecordSchema>;
