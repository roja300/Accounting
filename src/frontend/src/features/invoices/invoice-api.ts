export interface CreateWorkRecordDto {
	description: string;
	value: number;
	currency: 'GBP' | 'EUR' | 'USD';
	comments?: string;
	client: string;
	project: string;
}

export interface WorkRecordDto extends Omit<CreateWorkRecordDto, 'comments'> {
	id: string;
	comments: string;
	isExported: boolean;
	createdAt: string;
}

export interface WorkRecordOptionsDto {
	clients: string[];
	projects: string[];
}

export interface WorkRecordFilters {
	dateAfter?: string;
	dateBefore?: string;
	project?: string;
	client?: string;
	search?: string;
	page: number;
	pageSize: number;
}

export interface WorkRecordPageDto {
	items: WorkRecordDto[];
	page: number;
	pageSize: number;
	totalItems: number;
	totalPages: number;
}

const invoicesPath = '/api/invoices';

export function createWorkRecord(
	workRecord: CreateWorkRecordDto,
	signal?: AbortSignal,
): Promise<WorkRecordDto> {
	return request<WorkRecordDto>(invoicesPath, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(workRecord),
		signal,
	});
}

export function getWorkRecord(
	id: string,
	signal?: AbortSignal,
): Promise<WorkRecordDto> {
	return request<WorkRecordDto>(`${invoicesPath}/${id}`, { signal });
}

export function updateWorkRecord(
	id: string,
	workRecord: CreateWorkRecordDto,
	signal?: AbortSignal,
): Promise<WorkRecordDto> {
	return request<WorkRecordDto>(`${invoicesPath}/${id}`, {
		method: 'PUT',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(workRecord),
		signal,
	});
}

export async function deleteWorkRecord(id: string): Promise<void> {
	const response = await fetch(`${invoicesPath}/${id}`, { method: 'DELETE' });
	if (!response.ok) {
		throw new Error(`Request failed with status ${response.status}.`);
	}
}

export function getWorkRecordOptions(
	signal?: AbortSignal,
): Promise<WorkRecordOptionsDto> {
	return request<WorkRecordOptionsDto>(`${invoicesPath}/options`, { signal });
}

export function getWorkRecords(
	filters: WorkRecordFilters,
	signal?: AbortSignal,
): Promise<WorkRecordPageDto> {
	const parameters = new URLSearchParams({
		page: filters.page.toString(),
		pageSize: filters.pageSize.toString(),
	});

	for (const [name, value] of Object.entries(filters)) {
		if (name !== 'page' && name !== 'pageSize' && value) {
			parameters.set(name, value.toString());
		}
	}

	return request<WorkRecordPageDto>(
		`${invoicesPath}?${parameters.toString()}`,
		{ signal },
	);
}

async function request<T>(input: string, init: RequestInit): Promise<T> {
	const response = await fetch(input, init);
	if (!response.ok) {
		throw new Error(`Request failed with status ${response.status}.`);
	}

	return response.json() as Promise<T>;
}
