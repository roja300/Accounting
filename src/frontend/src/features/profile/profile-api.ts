export interface BusinessProfileDto {
	businessType: 'soleTrader' | 'limitedCompany';
	legalName: string;
	tradingName?: string;
	email: string;
	phone?: string;
	addressLine1: string;
	addressLine2?: string;
	city: string;
	county?: string;
	postcode: string;
	companyNumber?: string;
	vatNumber?: string;
}

export interface BusinessProfileResponseDto extends BusinessProfileDto {
	tradingName: string;
	phone: string;
	addressLine2: string;
	county: string;
	companyNumber: string;
	vatNumber: string;
	updatedAt: string;
}

export interface ClientDto {
	name: string;
	contactName?: string;
	email: string;
	phone?: string;
	addressLine1: string;
	addressLine2?: string;
	city: string;
	county?: string;
	postcode: string;
}

export interface ClientResponseDto extends ClientDto {
	id: string;
	contactName: string;
	phone: string;
	addressLine2: string;
	county: string;
	createdAt: string;
	updatedAt: string;
}

export type InvoiceStyle = 'basic' | 'modern' | 'minimal';

export interface InvoiceConfigurationResponseDto {
	style: InvoiceStyle;
	hasLogo: boolean;
	logoFileName: string;
	updatedAt?: string;
}

const profilePath = '/api/profile';

export async function getBusinessProfile(
	signal?: AbortSignal,
): Promise<BusinessProfileResponseDto | undefined> {
	const response = await fetch(`${profilePath}/business`, { signal });
	if (response.status === 404) return undefined;
	return readResponse<BusinessProfileResponseDto>(response);
}

export function saveBusinessProfile(
	profile: BusinessProfileDto,
): Promise<BusinessProfileResponseDto> {
	return request(`${profilePath}/business`, 'PUT', profile);
}

export async function getClients(
	signal?: AbortSignal,
): Promise<ClientResponseDto[]> {
	const response = await fetch(`${profilePath}/clients`, { signal });
	return readResponse<ClientResponseDto[]>(response);
}

export async function getInvoiceConfiguration(
	signal?: AbortSignal,
): Promise<InvoiceConfigurationResponseDto> {
	const response = await fetch(`${profilePath}/invoice-configuration`, {
		signal,
	});
	return readResponse<InvoiceConfigurationResponseDto>(response);
}

export function saveInvoiceConfiguration(
	style: InvoiceStyle,
): Promise<InvoiceConfigurationResponseDto> {
	return request(`${profilePath}/invoice-configuration`, 'PUT', { style });
}

export async function uploadInvoiceLogo(
	logo: File,
): Promise<InvoiceConfigurationResponseDto> {
	const body = new FormData();
	body.append('logo', logo);
	const response = await fetch(`${profilePath}/invoice-configuration/logo`, {
		method: 'PUT',
		body,
	});
	return readResponse<InvoiceConfigurationResponseDto>(response);
}

export async function deleteInvoiceLogo(): Promise<InvoiceConfigurationResponseDto> {
	const response = await fetch(`${profilePath}/invoice-configuration/logo`, {
		method: 'DELETE',
	});
	return readResponse<InvoiceConfigurationResponseDto>(response);
}

export const invoiceLogoUrl = `${profilePath}/invoice-configuration/logo`;

export function createClient(client: ClientDto): Promise<ClientResponseDto> {
	return request(`${profilePath}/clients`, 'POST', client);
}

export function updateClient(
	id: string,
	client: ClientDto,
): Promise<ClientResponseDto> {
	return request(`${profilePath}/clients/${id}`, 'PUT', client);
}

export async function deleteClient(id: string): Promise<void> {
	const response = await fetch(`${profilePath}/clients/${id}`, {
		method: 'DELETE',
	});
	if (!response.ok) throw await responseError(response);
}

async function request<T>(
	input: string,
	method: 'POST' | 'PUT',
	body: unknown,
): Promise<T> {
	const response = await fetch(input, {
		method,
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(body),
	});
	return readResponse<T>(response);
}

async function readResponse<T>(response: Response): Promise<T> {
	if (!response.ok) throw await responseError(response);
	return response.json() as Promise<T>;
}

async function responseError(response: Response): Promise<Error> {
	const fallback = `Request failed with status ${response.status}.`;
	try {
		const problem = (await response.json()) as { title?: string };
		return new Error(problem.title || fallback);
	} catch {
		return new Error(fallback);
	}
}
