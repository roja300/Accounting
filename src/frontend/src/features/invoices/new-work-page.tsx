import { BriefcaseBusinessIcon, CheckCircle2Icon } from 'lucide-react';
import { type FormEvent, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

import { Button } from '@/components/ui/button';
import {
	Card,
	CardContent,
	CardDescription,
	CardFooter,
	CardHeader,
	CardTitle,
} from '@/components/ui/card';
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from '@/components/ui/dialog';
import {
	type CreateWorkRecordDto,
	createWorkRecord,
	getWorkRecordOptions,
	type WorkRecordDto,
} from '@/features/invoices/invoice-api';

type WorkRecordField = keyof CreateWorkRecordDto;
type FormErrors = Partial<Record<WorkRecordField, string>>;

const descriptionMaximumLength = 200;
const commentsMaximumLength = 1_000;
const nameMaximumLength = 100;
const maximumValue = 9_999_999_999.99;
const supportedCurrencies = ['GBP', 'EUR', 'USD'] as const;
const inputClassName =
	'w-full rounded-lg border border-input bg-background px-3 py-2 text-sm outline-none transition-shadow placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 aria-invalid:border-destructive aria-invalid:ring-3 aria-invalid:ring-destructive/20';

export function NewWorkPage() {
	const navigate = useNavigate();
	const [client, setClient] = useState('');
	const [project, setProject] = useState('');
	const [clients, setClients] = useState<string[]>([]);
	const [projects, setProjects] = useState<string[]>([]);
	const [errors, setErrors] = useState<FormErrors>({});
	const [submitError, setSubmitError] = useState<string>();
	const [isSubmitting, setIsSubmitting] = useState(false);
	const [createdRecord, setCreatedRecord] = useState<WorkRecordDto>();

	useEffect(() => {
		const controller = new AbortController();

		getWorkRecordOptions(controller.signal)
			.then((options) => {
				setClients(options.clients);
				setProjects(options.projects);
			})
			.catch((error: unknown) => {
				if (
					!(
						error instanceof DOMException &&
						error.name === 'AbortError'
					)
				) {
					setClients([]);
					setProjects([]);
				}
			});

		return () => controller.abort();
	}, []);

	async function handleSubmit(event: FormEvent<HTMLFormElement>) {
		event.preventDefault();
		const formData = new FormData(event.currentTarget);
		const valueInput = readFormValue(formData, 'value');
		const workRecord: CreateWorkRecordDto = {
			description: readFormValue(formData, 'description').trim(),
			value: Number(valueInput),
			currency: readFormValue(formData, 'currency'),
			comments: readFormValue(formData, 'comments').trim(),
			client: client.trim(),
			project: project.trim(),
		};
		const validationErrors = validateWorkRecord(workRecord, valueInput);

		setErrors(validationErrors);
		setSubmitError(undefined);
		if (Object.keys(validationErrors).length > 0) {
			return;
		}

		setIsSubmitting(true);
		try {
			setCreatedRecord(await createWorkRecord(workRecord));
		} catch {
			setSubmitError(
				'The work record could not be saved. Please try again.',
			);
		} finally {
			setIsSubmitting(false);
		}
	}

	return (
		<section
			aria-labelledby="new-work-title"
			className="min-h-[calc(100svh-4rem)] bg-muted/30 px-4 py-10 sm:px-6 lg:px-8"
		>
			<div className="mx-auto w-full max-w-3xl">
				<div className="mb-6">
					<p className="mb-2 text-sm font-medium text-muted-foreground">
						Invoices
					</p>
					<h1
						id="new-work-title"
						className="text-3xl font-semibold tracking-tight"
					>
						New work
					</h1>
					<p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
						Record completed work so it is ready for your next
						invoice.
					</p>
				</div>

				<Card className="relative shadow-sm before:absolute before:inset-y-0 before:left-0 before:w-1 before:bg-primary">
					<CardHeader className="border-b pl-6 sm:pl-7">
						<div className="flex items-start gap-3">
							<div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground shadow-sm">
								<BriefcaseBusinessIcon
									className="size-5"
									aria-hidden="true"
								/>
							</div>
							<div>
								<CardTitle className="text-lg">
									Work details
								</CardTitle>
								<CardDescription className="mt-1 leading-5">
									All fields are required. Existing clients
									and projects appear as you type.
								</CardDescription>
							</div>
						</div>
					</CardHeader>

					<form
						onSubmit={handleSubmit}
						noValidate
					>
						<CardContent className="grid gap-5 pl-6 sm:grid-cols-2 sm:pl-7">
							<div className="grid gap-2 sm:col-span-2">
								<label
									htmlFor="description"
									className="text-sm font-medium"
								>
									Description
								</label>
								<textarea
									id="description"
									name="description"
									rows={3}
									maxLength={descriptionMaximumLength}
									aria-invalid={Boolean(errors.description)}
									aria-describedby={
										errors.description
											? 'description-error'
											: undefined
									}
									className={inputClassName}
								/>
								<FieldError
									id="description-error"
									message={errors.description}
								/>
							</div>

							<div className="grid gap-2">
								<label
									htmlFor="value"
									className="text-sm font-medium"
								>
									Value
								</label>
								<input
									id="value"
									name="value"
									type="number"
									min="0.01"
									max={maximumValue}
									step="0.01"
									inputMode="decimal"
									aria-invalid={Boolean(errors.value)}
									aria-describedby={
										errors.value ? 'value-error' : undefined
									}
									className={inputClassName}
								/>
								<FieldError
									id="value-error"
									message={errors.value}
								/>
							</div>

							<div className="grid gap-2">
								<label
									htmlFor="currency"
									className="text-sm font-medium"
								>
									Currency
								</label>
								<select
									id="currency"
									name="currency"
									defaultValue=""
									aria-invalid={Boolean(errors.currency)}
									aria-describedby={
										errors.currency
											? 'currency-error'
											: undefined
									}
									className={inputClassName}
								>
									<option
										value=""
										disabled
									>
										Select a currency
									</option>
									{supportedCurrencies.map((currency) => (
										<option
											key={currency}
											value={currency}
										>
											{currency}
										</option>
									))}
								</select>
								<FieldError
									id="currency-error"
									message={errors.currency}
								/>
							</div>

							<NameField
								id="client"
								label="Client"
								value={client}
								onChange={setClient}
								options={clients}
								error={errors.client}
							/>
							<NameField
								id="project"
								label="Project"
								value={project}
								onChange={setProject}
								options={projects}
								error={errors.project}
							/>

							<div className="grid gap-2 sm:col-span-2">
								<label
									htmlFor="comments"
									className="text-sm font-medium"
								>
									Additional comments
								</label>
								<textarea
									id="comments"
									name="comments"
									rows={4}
									maxLength={commentsMaximumLength}
									aria-invalid={Boolean(errors.comments)}
									aria-describedby={
										errors.comments
											? 'comments-error'
											: undefined
									}
									className={inputClassName}
								/>
								<FieldError
									id="comments-error"
									message={errors.comments}
								/>
							</div>

							{submitError ? (
								<p
									role="alert"
									className="text-sm text-destructive sm:col-span-2"
								>
									{submitError}
								</p>
							) : null}
						</CardContent>

						<CardFooter className="justify-between gap-3 border-t pl-6 sm:pl-7">
							<div className="flex items-center gap-2 text-xs text-muted-foreground">
								<CheckCircle2Icon
									className="size-4 text-foreground"
									aria-hidden="true"
								/>
								Saved work is ready for future invoicing.
							</div>
							<div className="flex items-center gap-2">
								<Link
									to="/invoices"
									className="rounded-lg px-3 py-2 text-sm font-medium hover:bg-muted focus-visible:outline-none focus-visible:ring-3 focus-visible:ring-ring/50"
								>
									Cancel
								</Link>
								<Button
									type="submit"
									disabled={isSubmitting}
								>
									{isSubmitting ? 'Saving…' : 'Save work'}
								</Button>
							</div>
						</CardFooter>
					</form>
				</Card>
			</div>

			<Dialog open={Boolean(createdRecord)}>
				<DialogContent showCloseButton={false}>
					<DialogHeader>
						<DialogTitle>Work record saved</DialogTitle>
						<DialogDescription>
							{createdRecord?.description} is now stored and ready
							to invoice.
						</DialogDescription>
					</DialogHeader>
					<DialogFooter>
						<Button onClick={() => navigate('/invoices')}>
							Return to invoices
						</Button>
					</DialogFooter>
				</DialogContent>
			</Dialog>
		</section>
	);
}

interface NameFieldProps {
	id: 'client' | 'project';
	label: string;
	value: string;
	onChange: (value: string) => void;
	options: string[];
	error?: string;
}

function NameField({
	id,
	label,
	value,
	onChange,
	options,
	error,
}: NameFieldProps) {
	const normalizedValue = value.trim().toLocaleLowerCase();
	const willCreate =
		normalizedValue.length > 0 &&
		!options.some(
			(option) => option.trim().toLocaleLowerCase() === normalizedValue,
		);
	const hintId = `${id}-hint`;
	const errorId = `${id}-error`;

	return (
		<div className="grid gap-2">
			<label
				htmlFor={id}
				className="text-sm font-medium"
			>
				{label}
			</label>
			<input
				id={id}
				name={id}
				list={`${id}-options`}
				value={value}
				onChange={(event) => onChange(event.currentTarget.value)}
				maxLength={nameMaximumLength}
				aria-invalid={Boolean(error)}
				aria-describedby={error ? `${hintId} ${errorId}` : hintId}
				className={inputClassName}
			/>
			<datalist id={`${id}-options`}>
				{options.map((option) => (
					<option
						key={option}
						value={option}
					/>
				))}
			</datalist>
			<p
				id={hintId}
				className="text-xs text-muted-foreground"
			>
				{willCreate
					? `A new ${label.toLocaleLowerCase()} named “${value.trim()}” will be created.`
					: `Choose an existing ${label.toLocaleLowerCase()} or enter a new name.`}
			</p>
			<FieldError
				id={errorId}
				message={error}
			/>
		</div>
	);
}

function FieldError({ id, message }: { id: string; message?: string }) {
	if (!message) {
		return null;
	}

	return (
		<p
			id={id}
			className="text-xs text-destructive"
		>
			{message}
		</p>
	);
}

function readFormValue(formData: FormData, name: string) {
	const value = formData.get(name);
	return typeof value === 'string' ? value : '';
}

function validateWorkRecord(
	workRecord: CreateWorkRecordDto,
	valueInput: string,
): FormErrors {
	const errors: FormErrors = {};

	validateText(
		errors,
		'description',
		workRecord.description,
		'Description',
		descriptionMaximumLength,
	);
	if (!/^\d+(\.\d{1,2})?$/.test(valueInput) || workRecord.value <= 0) {
		errors.value =
			'Value must be greater than zero with up to two decimal places.';
	} else if (workRecord.value > maximumValue) {
		errors.value = `Value must not exceed ${maximumValue.toFixed(2)}.`;
	}
	if (
		!supportedCurrencies.some(
			(currency) => currency === workRecord.currency,
		)
	) {
		errors.currency = 'Currency is required.';
	}
	validateText(
		errors,
		'comments',
		workRecord.comments,
		'Comments',
		commentsMaximumLength,
	);
	validateText(
		errors,
		'client',
		workRecord.client,
		'Client',
		nameMaximumLength,
	);
	validateText(
		errors,
		'project',
		workRecord.project,
		'Project',
		nameMaximumLength,
	);

	return errors;
}

function validateText(
	errors: FormErrors,
	field: WorkRecordField,
	value: string,
	label: string,
	maximumLength: number,
) {
	if (!value.trim()) {
		errors[field] = `${label} is required.`;
	} else if (value.trim().length > maximumLength) {
		errors[field] =
			`${label} must be ${maximumLength} characters or fewer.`;
	}
}
