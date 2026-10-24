export type InvoiceStatus = 'exported' | 'sent' | 'paymentReceived' | 'deleted';

export interface ExportOptionDto {
	id: string;
	name: string;
}

export interface GenerateInvoiceOptionsDto {
	clients: ExportOptionDto[];
	projects: ExportOptionDto[];
}

export interface GenerateInvoiceDto {
	clientId: string;
	projectId?: string;
	currency: 'GBP' | 'EUR' | 'USD';
	issueDate: string;
	dueDate: string;
	notes?: string;
}

export interface GeneratedInvoiceLineDto {
	workRecordId: string;
	description: string;
	project: string;
	value: number;
}

export interface GeneratedInvoiceDto {
	id: string;
	invoiceNumber: string;
	clientId: string;
	clientName: string;
	issueDate: string;
	dueDate: string;
	currency: string;
	subtotal: number;
	notes: string;
	status: InvoiceStatus;
	lines: GeneratedInvoiceLineDto[];
	createdAt: string;
}

export interface GeneratedInvoicePageDto {
	items: GeneratedInvoiceDto[];
	page: number;
	pageSize: number;
	totalItems: number;
	totalPages: number;
}

export interface GeneratedInvoiceFilters {
	dateAfter?: string;
	dateBefore?: string;
	clientId?: string;
	status?: string;
	search?: string;
	page: number;
	pageSize: number;
}

const path = '/api/exported-invoices';

export function getGenerateInvoiceOptions(signal?: AbortSignal) {
	return request<GenerateInvoiceOptionsDto>(`${path}/options`, { signal });
}

export function generateInvoice(
	invoice: GenerateInvoiceDto,
	signal?: AbortSignal,
) {
	return request<GeneratedInvoiceDto>(path, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(invoice),
		signal,
	});
}

export function getGeneratedInvoices(
	filters: GeneratedInvoiceFilters,
	signal?: AbortSignal,
) {
	const parameters = new URLSearchParams({
		page: filters.page.toString(),
		pageSize: filters.pageSize.toString(),
	});
	for (const [name, value] of Object.entries(filters)) {
		if (name !== 'page' && name !== 'pageSize' && value) {
			parameters.set(name, value.toString());
		}
	}
	return request<GeneratedInvoicePageDto>(`${path}?${parameters}`, {
		signal,
	});
}

export function updateGeneratedInvoiceStatus(
	id: string,
	status: InvoiceStatus,
) {
	return request<GeneratedInvoiceDto>(`${path}/${id}/status`, {
		method: 'PATCH',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ status }),
	});
}

export async function deleteGeneratedInvoice(id: string) {
	const response = await fetch(`${path}/${id}`, { method: 'DELETE' });
	if (!response.ok)
		throw new Error(`Request failed with status ${response.status}.`);
}

export function generatedInvoicePdfUrl(id: string) {
	return `${path}/${id}/pdf`;
}

async function request<T>(input: string, init: RequestInit): Promise<T> {
	const response = await fetch(input, init);
	if (!response.ok) {
		const problem = await response.json().catch(() => undefined);
		const message = problem?.errors
			? Object.values(problem.errors).flat().join(' ')
			: `Request failed with status ${response.status}.`;
		throw new Error(message);
	}
	return response.json() as Promise<T>;
}
