import { CalculatorIcon, PencilIcon } from 'lucide-react';
import { type FormEvent, useEffect, useState } from 'react';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
	configureTaxYear,
	getTaxYears,
	type TaxYear,
	type TaxYearConfiguration,
} from '@/features/tax/tax-api';
import { cn } from '@/lib/utils';

const money = new Intl.NumberFormat('en-GB', {
	style: 'currency',
	currency: 'GBP',
});

export function TaxPage() {
	const [taxYears, setTaxYears] = useState<TaxYear[]>([]);
	const [selected, setSelected] = useState<TaxYear>();
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState('');

	useEffect(() => {
		const controller = new AbortController();
		getTaxYears(controller.signal)
			.then(setTaxYears)
			.catch((reason: unknown) => {
				if (!controller.signal.aborted) {
					setError(
						reason instanceof Error
							? reason.message
							: 'Unable to load tax years.',
					);
				}
			})
			.finally(() => {
				if (!controller.signal.aborted) setLoading(false);
			});
		return () => controller.abort();
	}, []);

	return (
		<section
			aria-label="Tax page"
			className="min-h-[calc(100svh-4rem)] bg-muted/30 px-4 py-8 sm:px-6 lg:px-8"
		>
			<div className="mx-auto max-w-screen-2xl space-y-6">
				<header className="space-y-2">
					<div className="flex items-center gap-3">
						<div className="rounded-lg border bg-background p-2.5 shadow-sm">
							<CalculatorIcon
								className="size-5"
								aria-hidden="true"
							/>
						</div>
						<div>
							<h1
								id="tax-page-title"
								className="text-3xl font-bold tracking-tight"
							>
								Tax years
							</h1>
							<p className="text-sm text-muted-foreground">
								Ongoing UK tax estimates from completed work and
								paid invoices.
							</p>
						</div>
					</div>
				</header>

				<div className="rounded-lg border bg-background px-4 py-3 text-sm text-muted-foreground">
					Estimates use GBP invoices and England, Wales and Northern
					Ireland defaults. Configure each draft with your employment,
					pension and allowance details. VAT is not included, and
					these figures are not a submitted tax return.
				</div>

				{loading && <p role="status">Loading tax years…</p>}
				{error && (
					<p
						role="alert"
						className="text-sm text-destructive"
					>
						{error}
					</p>
				)}
				{!(loading || error) && (
					<div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
						{taxYears.map((taxYear) => (
							<TaxYearCard
								key={taxYear.startYear}
								taxYear={taxYear}
								onConfigure={setSelected}
							/>
						))}
					</div>
				)}
			</div>

			{selected && (
				<TaxConfigurationDialog
					taxYear={selected}
					onClose={() => setSelected(undefined)}
					onSaved={(saved) => {
						setTaxYears((current) =>
							current.map((item) =>
								item.startYear === saved.startYear
									? saved
									: item,
							),
						);
						setSelected(undefined);
					}}
				/>
			)}
		</section>
	);
}

function TaxYearCard({
	taxYear,
	onConfigure,
}: {
	taxYear: TaxYear;
	onConfigure: (taxYear: TaxYear) => void;
}) {
	return (
		<Card className="gap-0 overflow-hidden py-0">
			<CardHeader className="border-b bg-muted/30 px-5 py-5">
				<div className="flex items-start justify-between gap-4">
					<div className="space-y-1">
						<h2 className="font-heading text-base font-medium leading-snug">
							{taxYear.startYear}/
							{String(taxYear.startYear + 1).slice(-2)} tax year
						</h2>
						<p className="text-sm text-muted-foreground">
							6 April {taxYear.startYear} – 5 April{' '}
							{taxYear.startYear + 1}
						</p>
					</div>
					<span
						className={cn(
							'rounded-full border px-2.5 py-1 text-xs font-medium capitalize',
							taxYear.status === 'configured'
								? 'bg-primary text-primary-foreground'
								: 'bg-background text-muted-foreground',
						)}
					>
						{taxYear.status}
					</span>
				</div>
			</CardHeader>
			<CardContent className="space-y-5 p-5">
				<div className="grid grid-cols-2 gap-4">
					<Figure
						label="Expected pay"
						value={taxYear.expectedPay}
					/>
					<Figure
						label="Total earned"
						value={taxYear.totalEarned}
					/>
				</div>
				<div className="rounded-lg bg-primary p-4 text-primary-foreground">
					<p className="text-xs font-medium uppercase tracking-wide opacity-75">
						Estimated amount to pay
					</p>
					<p className="mt-1 text-2xl font-bold">
						{money.format(taxYear.amountToPay)}
					</p>
				</div>
				<dl className="space-y-2 text-sm">
					<Breakdown
						label="Income Tax"
						value={taxYear.estimatedIncomeTax}
					/>
					<Breakdown
						label="Class 4 National Insurance"
						value={taxYear.estimatedNationalInsurance}
					/>
				</dl>
				<Button
					className="w-full"
					variant="outline"
					onClick={() => onConfigure(taxYear)}
				>
					<PencilIcon
						className="size-4"
						aria-hidden="true"
					/>
					Configure tax year
				</Button>
			</CardContent>
		</Card>
	);
}

function Figure({ label, value }: { label: string; value: number }) {
	return (
		<div>
			<p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
				{label}
			</p>
			<p className="mt-1 text-xl font-semibold tabular-nums">
				{money.format(value)}
			</p>
		</div>
	);
}

function Breakdown({ label, value }: { label: string; value: number }) {
	return (
		<div className="flex justify-between gap-3">
			<dt className="text-muted-foreground">{label}</dt>
			<dd className="font-medium tabular-nums">{money.format(value)}</dd>
		</div>
	);
}

function TaxConfigurationDialog({
	taxYear,
	onClose,
	onSaved,
}: {
	taxYear: TaxYear;
	onClose: () => void;
	onSaved: (taxYear: TaxYear) => void;
}) {
	const [configuration, setConfiguration] = useState(taxYear.configuration);
	const [saving, setSaving] = useState(false);
	const [error, setError] = useState('');

	async function submit(event: FormEvent) {
		event.preventDefault();
		setSaving(true);
		setError('');
		try {
			onSaved(await configureTaxYear(taxYear.startYear, configuration));
		} catch (reason) {
			setError(
				reason instanceof Error
					? reason.message
					: 'Unable to save the tax year.',
			);
		} finally {
			setSaving(false);
		}
	}

	function update(
		field: keyof TaxYearConfiguration,
		value: string,
		percent = false,
	) {
		const parsed = Number(value);
		setConfiguration((current) => ({
			...current,
			[field]: Number.isFinite(parsed)
				? percent
					? parsed / 100
					: parsed
				: 0,
		}));
	}

	return (
		<Dialog
			open
			onOpenChange={(open) => !open && onClose()}
		>
			<DialogContent className="max-h-[90svh] overflow-y-auto sm:max-w-2xl">
				<form
					onSubmit={submit}
					className="space-y-6"
				>
					<DialogHeader>
						<DialogTitle>
							Configure {taxYear.startYear}/
							{String(taxYear.startYear + 1).slice(-2)}
						</DialogTitle>
						<DialogDescription>
							Add other income and reliefs, then review the
							editable UK calculation defaults.
						</DialogDescription>
					</DialogHeader>

					<fieldset className="grid gap-4 sm:grid-cols-2">
						<legend className="mb-3 text-sm font-semibold">
							Your tax-year details
						</legend>
						<MoneyField
							label="Employment income"
							field="employmentIncome"
							value={configuration.employmentIncome}
							onChange={update}
						/>
						<MoneyField
							label="Tax already paid through employment"
							field="employmentTaxPaid"
							value={configuration.employmentTaxPaid}
							onChange={update}
						/>
						<MoneyField
							label="Pension contributions"
							field="pensionContributions"
							value={configuration.pensionContributions}
							onChange={update}
						/>
						<MoneyField
							label="Allowable business expenses"
							field="allowableExpenses"
							value={configuration.allowableExpenses}
							onChange={update}
						/>
						<MoneyField
							label="Other tax-free allowances"
							field="additionalAllowances"
							value={configuration.additionalAllowances}
							onChange={update}
						/>
					</fieldset>

					<details className="rounded-lg border p-4">
						<summary className="cursor-pointer text-sm font-semibold">
							UK rates and thresholds
						</summary>
						<div className="mt-4 grid gap-4 sm:grid-cols-2">
							<MoneyField
								label="Personal allowance"
								field="personalAllowance"
								value={configuration.personalAllowance}
								onChange={update}
							/>
							<MoneyField
								label="Allowance taper threshold"
								field="personalAllowanceTaperThreshold"
								value={
									configuration.personalAllowanceTaperThreshold
								}
								onChange={update}
							/>
							<MoneyField
								label="Basic-rate band"
								field="basicRateLimit"
								value={configuration.basicRateLimit}
								onChange={update}
							/>
							<MoneyField
								label="Additional-rate threshold"
								field="higherRateLimit"
								value={configuration.higherRateLimit}
								onChange={update}
							/>
							<RateField
								label="Basic rate"
								field="basicRate"
								value={configuration.basicRate}
								onChange={update}
							/>
							<RateField
								label="Higher rate"
								field="higherRate"
								value={configuration.higherRate}
								onChange={update}
							/>
							<RateField
								label="Additional rate"
								field="additionalRate"
								value={configuration.additionalRate}
								onChange={update}
							/>
							<MoneyField
								label="NI lower profits limit"
								field="nationalInsuranceLowerLimit"
								value={
									configuration.nationalInsuranceLowerLimit
								}
								onChange={update}
							/>
							<MoneyField
								label="NI upper profits limit"
								field="nationalInsuranceUpperLimit"
								value={
									configuration.nationalInsuranceUpperLimit
								}
								onChange={update}
							/>
							<RateField
								label="NI main rate"
								field="nationalInsuranceMainRate"
								value={configuration.nationalInsuranceMainRate}
								onChange={update}
							/>
							<RateField
								label="NI additional rate"
								field="nationalInsuranceAdditionalRate"
								value={
									configuration.nationalInsuranceAdditionalRate
								}
								onChange={update}
							/>
						</div>
					</details>

					{error && (
						<p
							role="alert"
							className="text-sm text-destructive"
						>
							{error}
						</p>
					)}
					<DialogFooter>
						<Button
							type="button"
							variant="outline"
							onClick={onClose}
						>
							Cancel
						</Button>
						<Button
							type="submit"
							disabled={saving}
						>
							{saving ? 'Saving…' : 'Save configuration'}
						</Button>
					</DialogFooter>
				</form>
			</DialogContent>
		</Dialog>
	);
}

type UpdateConfiguration = (
	field: keyof TaxYearConfiguration,
	value: string,
	percent?: boolean,
) => void;

function MoneyField({
	label,
	field,
	value,
	onChange,
}: {
	label: string;
	field: keyof TaxYearConfiguration;
	value: number;
	onChange: UpdateConfiguration;
}) {
	return (
		<NumberField
			label={label}
			field={field}
			value={value}
			step="0.01"
			onChange={onChange}
		/>
	);
}

function RateField({
	label,
	field,
	value,
	onChange,
}: {
	label: string;
	field: keyof TaxYearConfiguration;
	value: number;
	onChange: UpdateConfiguration;
}) {
	return (
		<NumberField
			label={`${label} (%)`}
			field={field}
			value={value * 100}
			step="0.01"
			percent
			onChange={onChange}
		/>
	);
}

function NumberField({
	label,
	field,
	value,
	step,
	percent = false,
	onChange,
}: {
	label: string;
	field: keyof TaxYearConfiguration;
	value: number;
	step: string;
	percent?: boolean;
	onChange: UpdateConfiguration;
}) {
	return (
		<div className="grid gap-2">
			<Label htmlFor={`tax-${field}`}>{label}</Label>
			<Input
				id={`tax-${field}`}
				type="number"
				min="0"
				max={percent ? '100' : undefined}
				step={step}
				value={value}
				onChange={(event) =>
					onChange(field, event.target.value, percent)
				}
			/>
		</div>
	);
}
