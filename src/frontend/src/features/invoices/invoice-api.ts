export interface CreateWorkRecordDto {
	description: string;
	value: number;
	currency: string;
	comments?: string;
	client: string;
	project: string;
}

export interface WorkRecordDto extends Omit<CreateWorkRecordDto, 'comments'> {
	id: string;
	comments: string;
	createdAt: string;
}

export interface WorkRecordOptionsDto {
	clients: string[];
	projects: string[];
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

export function getWorkRecordOptions(
	signal?: AbortSignal,
): Promise<WorkRecordOptionsDto> {
	return request<WorkRecordOptionsDto>(`${invoicesPath}/options`, { signal });
}

async function request<T>(input: string, init: RequestInit): Promise<T> {
	const response = await fetch(input, init);
	if (!response.ok) {
		throw new Error(`Request failed with status ${response.status}.`);
	}

	return response.json() as Promise<T>;
}
