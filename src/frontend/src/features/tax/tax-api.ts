export interface TaxYearConfiguration {
	employmentIncome: number;
	employmentTaxPaid: number;
	pensionContributions: number;
	allowableExpenses: number;
	additionalAllowances: number;
	personalAllowance: number;
	personalAllowanceTaperThreshold: number;
	basicRateLimit: number;
	higherRateLimit: number;
	basicRate: number;
	higherRate: number;
	additionalRate: number;
	nationalInsuranceLowerLimit: number;
	nationalInsuranceUpperLimit: number;
	nationalInsuranceMainRate: number;
	nationalInsuranceAdditionalRate: number;
}

export interface TaxYear {
	startYear: number;
	startsOn: string;
	endsOn: string;
	status: 'draft' | 'configured';
	expectedPay: number;
	totalEarned: number;
	estimatedIncomeTax: number;
	estimatedNationalInsurance: number;
	amountToPay: number;
	configuration: TaxYearConfiguration;
}

const taxYearsPath = '/api/tax-years';

export async function getTaxYears(signal?: AbortSignal): Promise<TaxYear[]> {
	const response = await fetch(taxYearsPath, { signal });
	return readResponse<TaxYear[]>(response);
}

export async function configureTaxYear(
	startYear: number,
	configuration: TaxYearConfiguration,
): Promise<TaxYear> {
	const response = await fetch(`${taxYearsPath}/${startYear}`, {
		method: 'PUT',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(configuration),
	});
	return readResponse<TaxYear>(response);
}

async function readResponse<T>(response: Response): Promise<T> {
	if (response.ok) return response.json() as Promise<T>;
	let message = `Request failed with status ${response.status}.`;
	try {
		const problem = (await response.json()) as { title?: string };
		message = problem.title || message;
	} catch {
		// Keep the status-based fallback when the response is not JSON.
	}
	throw new Error(message);
}
