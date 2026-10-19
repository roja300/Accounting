import { zodResolver } from '@hookform/resolvers/zod';
import {
	BriefcaseBusinessIcon,
	CheckCircle2Icon,
	CirclePlusIcon,
	InfoIcon,
} from 'lucide-react';
import type { Ref } from 'react';
import { useEffect, useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { Link, useNavigate, useParams } from 'react-router-dom';

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
	Combobox,
	ComboboxContent,
	ComboboxEmpty,
	ComboboxInput,
	ComboboxItem,
	ComboboxList,
} from '@/components/ui/combobox';
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from '@/components/ui/dialog';
import {
	Field,
	FieldDescription,
	FieldError,
	FieldGroup,
	FieldLabel,
} from '@/components/ui/field';
import { Input } from '@/components/ui/input';
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import {
	COMMENTS_MAX_LENGTH,
	type CreateWorkRecordDto,
	type CreateWorkRecordFormValues,
	createWorkRecordSchema,
	currencyOptions,
	DESCRIPTION_MAX_LENGTH,
	MAXIMUM_VALUE,
	RELATED_RECORD_MAX_LENGTH,
} from '@/features/invoices/create-work-record';
import {
	createWorkRecord,
	getWorkRecord,
	getWorkRecordOptions,
	updateWorkRecord,
	type WorkRecordDto,
} from '@/features/invoices/invoice-api';

const monetaryValuePattern = /^\d*(?:\.\d{0,2})?$/;

function isAbortError(error: unknown) {
	return error instanceof DOMException && error.name === 'AbortError';
}

type CreatableComboboxFieldProps = {
	label: string;
	name: 'client' | 'project';
	placeholder: string;
	options: readonly string[];
	value: string;
	onChange: (value: string) => void;
	onBlur: () => void;
	inputRef: Ref<HTMLInputElement>;
	invalid: boolean;
	error?: string;
};

function CreatableComboboxField({
	label,
	name,
	placeholder,
	options,
	value,
	onChange,
	onBlur,
	inputRef,
	invalid,
	error,
}: CreatableComboboxFieldProps) {
	const trimmedValue = value.trim();
	const matchesExistingOption = options.some(
		(option) =>
			option.toLocaleLowerCase() === trimmedValue.toLocaleLowerCase(),
	);
	const willCreate = trimmedValue.length > 0 && !matchesExistingOption;
	const availableOptions = willCreate
		? [...options, trimmedValue]
		: [...options];
	const descriptionId = `${name}-description`;
	const errorId = `${name}-error`;

	return (
		<Field data-invalid={invalid}>
			<FieldLabel htmlFor={name}>{label}</FieldLabel>
			<FieldDescription id={descriptionId}>
				{willCreate ? (
					<span className="inline-flex items-center gap-1.5 text-foreground">
						<InfoIcon
							className="size-3.5"
							aria-hidden="true"
						/>
						A new {name} named “{trimmedValue}” will be created.
					</span>
				) : (
					`Choose an existing ${name} or enter a new one.`
				)}
			</FieldDescription>
			<Combobox<string>
				items={availableOptions}
				inputValue={value}
				required
				onInputValueChange={(nextValue, eventDetails) => {
					if (eventDetails.reason === 'input-change') {
						onChange(nextValue);
					}
				}}
				onValueChange={(nextValue) => {
					if (nextValue !== null) {
						onChange(nextValue);
					}
				}}
			>
				<ComboboxInput
					id={name}
					name={name}
					placeholder={placeholder}
					maxLength={RELATED_RECORD_MAX_LENGTH}
					required
					aria-invalid={invalid}
					aria-describedby={`${descriptionId}${invalid ? ` ${errorId}` : ''}`}
					onBlur={onBlur}
					ref={inputRef}
					className="w-full"
				/>
				<ComboboxContent>
					<ComboboxEmpty>No matching {name}s found.</ComboboxEmpty>
					<ComboboxList>
						{(option: string) => {
							const isCreateOption =
								willCreate && option === trimmedValue;

							return (
								<ComboboxItem
									key={option}
									value={option}
								>
									{isCreateOption && (
										<CirclePlusIcon aria-hidden="true" />
									)}
									{isCreateOption
										? `Create “${option}”`
										: option}
								</ComboboxItem>
							);
						}}
					</ComboboxList>
				</ComboboxContent>
			</Combobox>
			{invalid && <FieldError id={errorId}>{error}</FieldError>}
		</Field>
	);
}

function WorkPageHeader({ isEditing }: { isEditing: boolean }) {
	return (
		<div className="mb-6">
			<p className="mb-2 text-sm font-medium text-muted-foreground">
				Invoices
			</p>
			<h1
				id="work-title"
				className="text-3xl font-semibold tracking-tight"
			>
				{isEditing ? 'Edit work' : 'New work'}
			</h1>
			<p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
				{isEditing
					? 'Update the completed work details for your next invoice.'
					: 'Record completed work so it is ready for your next invoice.'}
			</p>
		</div>
	);
}

function EditRecordStatus({
	loadError,
	canRetry,
	onRetry,
}: {
	loadError: boolean;
	canRetry: boolean;
	onRetry: () => void;
}) {
	return (
		<section
			aria-labelledby="work-title"
			className="min-h-[calc(100svh-4rem)] bg-muted/30 px-4 py-10 sm:px-6 lg:px-8"
		>
			<div className="mx-auto w-full max-w-3xl">
				<WorkPageHeader isEditing />
				<Card
					className="items-center py-12 text-center shadow-sm"
					role={loadError ? 'alert' : 'status'}
				>
					<CardHeader>
						<CardTitle>
							{loadError
								? 'Work record could not be loaded'
								: 'Loading work record…'}
						</CardTitle>
						{loadError && (
							<CardDescription>
								Check your connection and try again.
							</CardDescription>
						)}
					</CardHeader>
					{loadError && canRetry && (
						<CardContent>
							<Button
								type="button"
								onClick={onRetry}
							>
								Try again
							</Button>
						</CardContent>
					)}
				</Card>
			</div>
		</section>
	);
}

export function NewWorkPage({ mode = 'create' }: { mode?: 'create' | 'edit' }) {
	const navigate = useNavigate();
	const { recordId } = useParams<{ recordId: string }>();
	const isEditing = mode === 'edit';
	const [clients, setClients] = useState<string[]>([]);
	const [projects, setProjects] = useState<string[]>([]);
	const [submitError, setSubmitError] = useState<string>();
	const [successDialogOpen, setSuccessDialogOpen] = useState(false);
	const [savedRecord, setSavedRecord] = useState<WorkRecordDto>();
	const [recordLoading, setRecordLoading] = useState(isEditing);
	const [recordLoadError, setRecordLoadError] = useState(false);
	const [recordLoadRetry, setRecordLoadRetry] = useState(0);
	const form = useForm<
		CreateWorkRecordFormValues,
		unknown,
		CreateWorkRecordDto
	>({
		resolver: zodResolver(createWorkRecordSchema),
		defaultValues: {
			description: '',
			comments: '',
			client: '',
			project: '',
		},
	});
	const { reset } = form;

	useEffect(() => {
		const controller = new AbortController();

		getWorkRecordOptions(controller.signal)
			.then((options) => {
				setClients(options.clients);
				setProjects(options.projects);
			})
			.catch((error: unknown) => {
				if (!isAbortError(error)) {
					setClients([]);
					setProjects([]);
				}
			});

		return () => controller.abort();
	}, []);

	useEffect(() => {
		if (!isEditing) return;

		if (!recordId) {
			setRecordLoading(false);
			setRecordLoadError(true);
			return;
		}

		const controller = new AbortController();
		setRecordLoading(true);
		setRecordLoadError(false);

		getWorkRecord(recordId, controller.signal)
			.then((record) => {
				reset({
					description: record.description,
					value: record.value,
					currency: record.currency,
					comments: record.comments,
					client: record.client,
					project: record.project,
				});
			})
			.catch((error: unknown) => {
				if (!isAbortError(error)) {
					setRecordLoadError(true);
				}
			})
			.finally(() => {
				if (!controller.signal.aborted) setRecordLoading(false);
			});

		return () => controller.abort();
	}, [isEditing, recordId, recordLoadRetry, reset]);

	async function handleSuccessfulSubmit(workRecord: CreateWorkRecordDto) {
		setSubmitError(undefined);

		try {
			const saved =
				isEditing && recordId
					? await updateWorkRecord(recordId, workRecord)
					: await createWorkRecord(workRecord);
			setSavedRecord(saved);
			setSuccessDialogOpen(true);
		} catch {
			setSubmitError(
				`The work record could not be ${isEditing ? 'updated' : 'saved'}. Please try again.`,
			);
		}
	}

	function handleDialogOpenChange(open: boolean) {
		setSuccessDialogOpen(open);

		if (!open) {
			navigate('/invoices');
		}
	}

	if (isEditing && (recordLoading || recordLoadError)) {
		return (
			<EditRecordStatus
				loadError={recordLoadError}
				canRetry={Boolean(recordId)}
				onRetry={() => setRecordLoadRetry((count) => count + 1)}
			/>
		);
	}

	return (
		<section
			aria-labelledby="work-title"
			className="min-h-[calc(100svh-4rem)] bg-muted/30 px-4 py-10 sm:px-6 lg:px-8"
		>
			<div className="mx-auto w-full max-w-3xl">
				<WorkPageHeader isEditing={isEditing} />

				<form
					onSubmit={form.handleSubmit(handleSuccessfulSubmit)}
					noValidate
				>
					<Card
						role="region"
						aria-labelledby="work-details-title"
						className="shadow-sm"
					>
						<CardHeader className="border-b pl-6 sm:pl-7">
							<div className="flex items-start gap-3">
								<div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground shadow-sm">
									<BriefcaseBusinessIcon
										className="size-5"
										aria-hidden="true"
									/>
								</div>
								<div>
									<CardTitle
										id="work-details-title"
										className="text-lg"
									>
										Work details
									</CardTitle>
									<CardDescription className="mt-1 leading-5">
										Add the information that will appear on
										the invoice.
									</CardDescription>
								</div>
							</div>
						</CardHeader>

						<CardContent className="pl-6 sm:pl-7">
							<FieldGroup>
								<Controller
									name="description"
									control={form.control}
									render={({ field, fieldState }) => (
										<Field
											data-invalid={fieldState.invalid}
										>
											<FieldLabel htmlFor={field.name}>
												Description
											</FieldLabel>
											<FieldDescription id="description-limit">
												Maximum {DESCRIPTION_MAX_LENGTH}{' '}
												characters.
											</FieldDescription>
											<Input
												{...field}
												id={field.name}
												placeholder="e.g. Prepare September management accounts"
												maxLength={
													DESCRIPTION_MAX_LENGTH
												}
												required
												aria-invalid={
													fieldState.invalid
												}
												aria-describedby={`description-limit${fieldState.invalid ? ' description-error' : ''}`}
											/>
											{fieldState.invalid && (
												<FieldError
													id="description-error"
													errors={[fieldState.error]}
												/>
											)}
										</Field>
									)}
								/>

								<div className="grid gap-5 sm:grid-cols-2">
									<Controller
										name="value"
										control={form.control}
										render={({ field, fieldState }) => (
											<Field
												data-invalid={
													fieldState.invalid
												}
											>
												<FieldLabel
													htmlFor={field.name}
												>
													Value
												</FieldLabel>
												<Input
													id={field.name}
													name={field.name}
													ref={field.ref}
													type="number"
													inputMode="decimal"
													min="0.01"
													max={MAXIMUM_VALUE}
													step="0.01"
													placeholder="0.00"
													required
													className="[appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none"
													value={field.value ?? ''}
													onBlur={field.onBlur}
													onKeyDown={(event) => {
														if (
															[
																'e',
																'E',
																'+',
																'-',
															].includes(
																event.key,
															)
														) {
															event.preventDefault();
														}
													}}
													onPaste={(event) => {
														if (
															!monetaryValuePattern.test(
																event.clipboardData.getData(
																	'text',
																),
															)
														) {
															event.preventDefault();
														}
													}}
													onChange={(event) =>
														monetaryValuePattern.test(
															event.target.value,
														) &&
														field.onChange(
															event.target
																.value === ''
																? undefined
																: event.target
																		.valueAsNumber,
														)
													}
													aria-invalid={
														fieldState.invalid
													}
													aria-describedby={
														fieldState.invalid
															? 'value-error'
															: undefined
													}
												/>
												{fieldState.invalid && (
													<FieldError
														id="value-error"
														errors={[
															fieldState.error,
														]}
													/>
												)}
											</Field>
										)}
									/>

									<Controller
										name="currency"
										control={form.control}
										render={({ field, fieldState }) => (
											<Field
												data-invalid={
													fieldState.invalid
												}
											>
												<FieldLabel
													htmlFor={field.name}
												>
													Currency
												</FieldLabel>
												<Select
													items={currencyOptions}
													name={field.name}
													value={field.value ?? null}
													required
													onValueChange={(value) =>
														field.onChange(value)
													}
												>
													<SelectTrigger
														id={field.name}
														className="w-full"
														aria-invalid={
															fieldState.invalid
														}
														aria-required="true"
														aria-describedby={
															fieldState.invalid
																? 'currency-error'
																: undefined
														}
														onBlur={field.onBlur}
													>
														<SelectValue placeholder="Select a currency" />
													</SelectTrigger>
													<SelectContent>
														{currencyOptions.map(
															(currency) => (
																<SelectItem
																	key={
																		currency.value
																	}
																	value={
																		currency.value
																	}
																>
																	{
																		currency.label
																	}
																</SelectItem>
															),
														)}
													</SelectContent>
												</Select>
												{fieldState.invalid && (
													<FieldError
														id="currency-error"
														errors={[
															fieldState.error,
														]}
													/>
												)}
											</Field>
										)}
									/>
								</div>

								<div className="grid gap-5 sm:grid-cols-2">
									<Controller
										name="client"
										control={form.control}
										render={({ field, fieldState }) => (
											<CreatableComboboxField
												label="Client"
												name="client"
												placeholder="Select or enter a client"
												options={clients}
												value={field.value}
												onChange={field.onChange}
												onBlur={field.onBlur}
												inputRef={field.ref}
												invalid={fieldState.invalid}
												error={
													fieldState.error?.message
												}
											/>
										)}
									/>

									<Controller
										name="project"
										control={form.control}
										render={({ field, fieldState }) => (
											<CreatableComboboxField
												label="Project"
												name="project"
												placeholder="Select or enter a project"
												options={projects}
												value={field.value}
												onChange={field.onChange}
												onBlur={field.onBlur}
												inputRef={field.ref}
												invalid={fieldState.invalid}
												error={
													fieldState.error?.message
												}
											/>
										)}
									/>
								</div>

								<Controller
									name="comments"
									control={form.control}
									render={({ field, fieldState }) => (
										<Field
											data-invalid={fieldState.invalid}
										>
											<FieldLabel htmlFor={field.name}>
												Additional comments{' '}
												<span className="font-normal text-muted-foreground">
													(optional)
												</span>
											</FieldLabel>
											<FieldDescription id="comments-limit">
												Maximum {COMMENTS_MAX_LENGTH}{' '}
												characters.
											</FieldDescription>
											<Textarea
												{...field}
												id={field.name}
												placeholder="Add any useful billing context or notes"
												maxLength={COMMENTS_MAX_LENGTH}
												aria-invalid={
													fieldState.invalid
												}
												aria-describedby={`comments-limit${fieldState.invalid ? ' comments-error' : ''}`}
												className="min-h-28 resize-y"
											/>
											{fieldState.invalid && (
												<FieldError
													id="comments-error"
													errors={[fieldState.error]}
												/>
											)}
										</Field>
									)}
								/>
							</FieldGroup>
							{submitError && (
								<FieldError className="mt-5">
									{submitError}
								</FieldError>
							)}
						</CardContent>

						<CardFooter className="flex-col items-stretch gap-4 border-t pl-6 sm:flex-row sm:items-center sm:justify-between sm:pl-7">
							<p className="inline-flex items-center gap-2 text-xs text-muted-foreground">
								<CheckCircle2Icon
									className="size-4 text-foreground"
									aria-hidden="true"
								/>
								Complete all required fields.
							</p>
							<div className="flex justify-end gap-2">
								<Button
									variant="outline"
									nativeButton={false}
									render={<Link to="/invoices" />}
								>
									Cancel
								</Button>
								<Button
									type="submit"
									disabled={form.formState.isSubmitting}
								>
									{form.formState.isSubmitting
										? 'Saving…'
										: isEditing
											? 'Save changes'
											: 'Save work'}
								</Button>
							</div>
						</CardFooter>
					</Card>
				</form>
			</div>

			<Dialog
				open={successDialogOpen}
				onOpenChange={handleDialogOpenChange}
			>
				<DialogContent>
					<DialogHeader>
						<div className="mb-1 flex size-10 items-center justify-center rounded-full bg-primary text-primary-foreground">
							<CheckCircle2Icon
								className="size-5"
								aria-hidden="true"
							/>
						</div>
						<DialogTitle>
							Work record {isEditing ? 'updated' : 'saved'}
						</DialogTitle>
						<DialogDescription>
							{savedRecord
								? `“${savedRecord.description}” was ${isEditing ? 'updated' : 'added to invoice'}.`
								: `The work record was ${isEditing ? 'updated' : 'added to invoice'}.`}
						</DialogDescription>
					</DialogHeader>
					<DialogFooter>
						<Button
							type="button"
							onClick={() => handleDialogOpenChange(false)}
						>
							Return to invoices
						</Button>
					</DialogFooter>
				</DialogContent>
			</Dialog>
		</section>
	);
}
