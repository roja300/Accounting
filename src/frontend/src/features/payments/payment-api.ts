export interface PaymentDto {
	id: string;
	importId: string;
	importFileName: string;
	transactionAt: string;
	description: string;
	counterparty: string;
	reference: string;
	amount: number;
	balance: number | null;
	currency: string;
}

export interface PaymentPageDto {
	items: PaymentDto[];
	page: number;
	pageSize: number;
	totalItems: number;
	totalPages: number;
}

export interface PaymentImportDto {
	id: string;
	fileName: string;
	rowCount: number;
	importedAt: string;
}

export interface PaymentFilters {
	dateAfter?: string;
	dateBefore?: string;
	search?: string;
	direction?: 'incoming' | 'outgoing' | '';
	importId?: string;
	page: number;
	pageSize: number;
}

export interface DeletePaymentsRequest {
	all?: boolean;
	dateAfter?: string;
	dateBefore?: string;
	importId?: string;
}

export class PaymentApiError extends Error {
	readonly errors: Record<string, string[]>;

	constructor(message: string, errors: Record<string, string[]> = {}) {
		super(message);
		this.errors = errors;
	}
}

const paymentsPath = '/api/payments';

export function getPayments(
	filters: PaymentFilters,
	signal?: AbortSignal,
): Promise<PaymentPageDto> {
	const parameters = new URLSearchParams({
		page: filters.page.toString(),
		pageSize: filters.pageSize.toString(),
	});
	for (const [name, value] of Object.entries(filters)) {
		if (name !== 'page' && name !== 'pageSize' && value) {
			parameters.set(name, value.toString());
		}
	}
	return request<PaymentPageDto>(`${paymentsPath}?${parameters}`, { signal });
}

export function getPaymentImports(
	signal?: AbortSignal,
): Promise<PaymentImportDto[]> {
	return request<PaymentImportDto[]>(`${paymentsPath}/imports`, { signal });
}

export function importPayments(file: File): Promise<PaymentImportDto> {
	const body = new FormData();
	body.append('file', file);
	return request<PaymentImportDto>(`${paymentsPath}/imports`, {
		method: 'POST',
		body,
	});
}

export async function deletePayments(
	selection: DeletePaymentsRequest,
): Promise<void> {
	const parameters = new URLSearchParams();
	for (const [name, value] of Object.entries(selection)) {
		if (value !== undefined && value !== '') {
			parameters.set(name, value.toString());
		}
	}
	await request<void>(`${paymentsPath}?${parameters}`, { method: 'DELETE' });
}

async function request<T>(input: string, init: RequestInit): Promise<T> {
	const response = await fetch(input, init);
	if (!response.ok) {
		let errors: Record<string, string[]> = {};
		try {
			const problem = (await response.json()) as {
				errors?: Record<string, string[]>;
			};
			errors = problem.errors ?? {};
		} catch {
			// A non-problem response still becomes a stable error for the UI.
		}
		throw new PaymentApiError(
			`Request failed with status ${response.status}.`,
			errors,
		);
	}
	if (response.status === 204) return undefined as T;
	return response.json() as Promise<T>;
}
