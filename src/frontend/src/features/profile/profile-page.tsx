import { zodResolver } from '@hookform/resolvers/zod';
import {
	Building2Icon,
	PencilIcon,
	PlusIcon,
	Trash2Icon,
	UsersIcon,
} from 'lucide-react';
import { useEffect, useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { z } from 'zod';

import { Button } from '@/components/ui/button';
import {
	Card,
	CardContent,
	CardDescription,
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
import { FieldDescription } from '@/components/ui/field';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from '@/components/ui/select';
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from '@/components/ui/table';
import {
	type BusinessProfileDto,
	type ClientDto,
	type ClientResponseDto,
	createClient,
	deleteClient,
	getBusinessProfile,
	getClients,
	saveBusinessProfile,
	updateClient,
} from '@/features/profile/profile-api';

const optionalText = (maximum: number) =>
	z
		.string()
		.trim()
		.max(maximum)
		.transform((value) => value || undefined);
const businessSchema = z
	.object({
		businessType: z.enum(['soleTrader', 'limitedCompany']),
		legalName: z.string().trim().min(1, 'Enter the legal name.').max(150),
		tradingName: optionalText(150),
		email: z.string().trim().email('Enter a valid email address.').max(254),
		phone: optionalText(30),
		addressLine1: z.string().trim().min(1, 'Enter the address.').max(100),
		addressLine2: optionalText(100),
		city: z.string().trim().min(1, 'Enter the town or city.').max(100),
		county: optionalText(100),
		postcode: z.string().trim().min(1, 'Enter the postcode.').max(10),
		companyNumber: optionalText(8),
		vatNumber: optionalText(12),
	})
	.superRefine((profile, context) => {
		if (
			profile.businessType === 'limitedCompany' &&
			!profile.companyNumber
		) {
			context.addIssue({
				code: z.ZodIssueCode.custom,
				path: ['companyNumber'],
				message: 'Enter the Companies House number.',
			});
		}
	});
const clientSchema = z.object({
	name: z.string().trim().min(1, 'Enter the customer name.').max(100),
	contactName: optionalText(100),
	email: z.string().trim().email('Enter a valid email address.').max(254),
	phone: optionalText(30),
	addressLine1: z.string().trim().min(1, 'Enter the address.').max(100),
	addressLine2: optionalText(100),
	city: z.string().trim().min(1, 'Enter the town or city.').max(100),
	county: optionalText(100),
	postcode: z.string().trim().min(1, 'Enter the postcode.').max(10),
});

type BusinessFormValues = z.input<typeof businessSchema>;
type ClientFormValues = z.input<typeof clientSchema>;

const emptyBusiness: BusinessFormValues = {
	businessType: 'soleTrader',
	legalName: '',
	tradingName: '',
	email: '',
	phone: '',
	addressLine1: '',
	addressLine2: '',
	city: '',
	county: '',
	postcode: '',
	companyNumber: '',
	vatNumber: '',
};
const emptyClient: ClientFormValues = {
	name: '',
	contactName: '',
	email: '',
	phone: '',
	addressLine1: '',
	addressLine2: '',
	city: '',
	county: '',
	postcode: '',
};

function isAbortError(error: unknown) {
	return error instanceof DOMException && error.name === 'AbortError';
}

export function ProfilePage() {
	const [clients, setClients] = useState<ClientResponseDto[]>([]);
	const [loading, setLoading] = useState(true);
	const [loadError, setLoadError] = useState(false);
	const [businessMessage, setBusinessMessage] = useState<string>();
	const [clientDialogOpen, setClientDialogOpen] = useState(false);
	const [editingClient, setEditingClient] = useState<ClientResponseDto>();
	const [clientError, setClientError] = useState<string>();
	const businessForm = useForm<
		BusinessFormValues,
		unknown,
		BusinessProfileDto
	>({
		resolver: zodResolver(businessSchema),
		defaultValues: emptyBusiness,
	});
	const clientForm = useForm<ClientFormValues, unknown, ClientDto>({
		resolver: zodResolver(clientSchema),
		defaultValues: emptyClient,
	});
	const { reset: resetBusiness } = businessForm;

	useEffect(() => {
		const controller = new AbortController();
		Promise.all([
			getBusinessProfile(controller.signal),
			getClients(controller.signal),
		])
			.then(([profile, loadedClients]) => {
				if (profile) resetBusiness(profile);
				setClients(loadedClients);
				setLoadError(false);
			})
			.catch((error: unknown) => {
				if (!isAbortError(error)) setLoadError(true);
			})
			.finally(() => {
				if (!controller.signal.aborted) setLoading(false);
			});
		return () => controller.abort();
	}, [resetBusiness]);

	async function submitBusiness(profile: BusinessProfileDto) {
		setBusinessMessage(undefined);
		try {
			const saved = await saveBusinessProfile(profile);
			businessForm.reset(saved);
			setBusinessMessage('Business information saved.');
		} catch {
			setBusinessMessage('Business information could not be saved.');
		}
	}

	function openClient(client?: ClientResponseDto) {
		setEditingClient(client);
		setClientError(undefined);
		clientForm.reset(client ?? emptyClient);
		setClientDialogOpen(true);
	}

	async function submitClient(client: ClientDto) {
		setClientError(undefined);
		try {
			const saved = editingClient
				? await updateClient(editingClient.id, client)
				: await createClient(client);
			setClients((current) =>
				editingClient
					? current
							.map((item) =>
								item.id === saved.id ? saved : item,
							)
							.sort((left, right) =>
								left.name.localeCompare(right.name),
							)
					: [...current, saved].sort((left, right) =>
							left.name.localeCompare(right.name),
						),
			);
			setClientDialogOpen(false);
		} catch (error) {
			setClientError(
				error instanceof Error
					? error.message
					: 'The customer could not be saved.',
			);
		}
	}

	async function removeClient(client: ClientResponseDto) {
		if (!window.confirm(`Delete ${client.name}?`)) return;
		try {
			await deleteClient(client.id);
			setClients((current) =>
				current.filter((item) => item.id !== client.id),
			);
		} catch (error) {
			window.alert(
				error instanceof Error
					? error.message
					: 'The customer could not be deleted.',
			);
		}
	}

	if (loading) {
		return (
			<p className="px-6 py-12 text-center text-muted-foreground">
				Loading profile…
			</p>
		);
	}

	if (loadError) {
		return (
			<p className="px-6 py-12 text-center text-destructive">
				Profile information could not be loaded.
			</p>
		);
	}

	return (
		<section
			aria-labelledby="profile-title"
			className="bg-muted/30 px-4 py-10 sm:px-6 lg:px-8"
		>
			<div className="mx-auto grid w-full max-w-5xl gap-8">
				<div>
					<p className="text-sm font-medium text-muted-foreground">
						Settings
					</p>
					<h1
						id="profile-title"
						className="text-3xl font-semibold tracking-tight"
					>
						Business profile
					</h1>
					<p className="mt-2 text-muted-foreground">
						Manage the details used on invoices and the customers
						you work with.
					</p>
				</div>

				<Card>
					<CardHeader className="border-b">
						<div className="flex items-start gap-3">
							<div className="flex size-10 items-center justify-center rounded-lg bg-primary text-primary-foreground">
								<Building2Icon
									className="size-5"
									aria-hidden="true"
								/>
							</div>
							<div>
								<CardTitle>Business information</CardTitle>
								<CardDescription>
									Legal, contact and registration details for
									UK invoices.
								</CardDescription>
							</div>
						</div>
					</CardHeader>
					<CardContent>
						<form
							className="grid gap-5"
							onSubmit={businessForm.handleSubmit(submitBusiness)}
							noValidate
						>
							<div className="grid gap-5 sm:grid-cols-2">
								<Controller
									name="businessType"
									control={businessForm.control}
									render={({ field }) => (
										<FormSelect
											label="Business type"
											description="Choose the legal structure that should appear on your invoices."
											name={field.name}
											value={field.value}
											onChange={field.onChange}
											onBlur={field.onBlur}
											options={[
												['soleTrader', 'Sole trader'],
												[
													'limitedCompany',
													'Limited company',
												],
											]}
										/>
									)}
								/>
								<FormInput
									label="Legal name"
									description="Your full personal name or registered company name."
									registration={businessForm.register(
										'legalName',
									)}
									error={
										businessForm.formState.errors.legalName
											?.message
									}
								/>
								<FormInput
									label="Trading name (optional)"
									description="The name customers know your business by, if different."
									registration={businessForm.register(
										'tradingName',
									)}
								/>
								<FormInput
									label="Email"
									description="Business contact email to show on invoices."
									type="email"
									registration={businessForm.register(
										'email',
									)}
									error={
										businessForm.formState.errors.email
											?.message
									}
								/>
								<FormInput
									label="Phone (optional)"
									description="Business contact number to show on invoices."
									type="tel"
									registration={businessForm.register(
										'phone',
									)}
								/>
								<FormInput
									label="Address line 1"
									description="Primary business or registered office address."
									registration={businessForm.register(
										'addressLine1',
									)}
									error={
										businessForm.formState.errors
											.addressLine1?.message
									}
								/>
								<FormInput
									label="Address line 2 (optional)"
									description="Additional address details, such as a building or suite."
									registration={businessForm.register(
										'addressLine2',
									)}
								/>
								<FormInput
									label="Town or city"
									description="Town or city for the business address."
									registration={businessForm.register('city')}
									error={
										businessForm.formState.errors.city
											?.message
									}
								/>
								<FormInput
									label="County (optional)"
									description="County for the business address, if applicable."
									registration={businessForm.register(
										'county',
									)}
								/>
								<FormInput
									label="Postcode"
									description="UK postcode for the business address."
									registration={businessForm.register(
										'postcode',
									)}
									error={
										businessForm.formState.errors.postcode
											?.message
									}
								/>
								<FormInput
									label="Company number"
									description="Companies House number; required for limited companies."
									registration={businessForm.register(
										'companyNumber',
									)}
									error={
										businessForm.formState.errors
											.companyNumber?.message
									}
								/>
								<FormInput
									label="VAT number (if registered)"
									description="VAT registration number to include on VAT invoices."
									registration={businessForm.register(
										'vatNumber',
									)}
								/>
							</div>
							<div className="flex items-center justify-end gap-4">
								<p
									role="status"
									className="text-sm text-muted-foreground"
								>
									{businessMessage}
								</p>
								<Button
									type="submit"
									disabled={
										businessForm.formState.isSubmitting
									}
								>
									{businessForm.formState.isSubmitting
										? 'Saving…'
										: 'Save business'}
								</Button>
							</div>
						</form>
					</CardContent>
				</Card>

				<Card>
					<CardHeader className="flex-row items-start justify-between border-b">
						<div className="flex items-start gap-3">
							<div className="flex size-10 items-center justify-center rounded-lg bg-primary text-primary-foreground">
								<UsersIcon
									className="size-5"
									aria-hidden="true"
								/>
							</div>
							<div>
								<CardTitle>Customers</CardTitle>
								<CardDescription>
									Configure invoice recipients before
									recording work.
								</CardDescription>
							</div>
						</div>
						<Button
							type="button"
							onClick={() => openClient()}
						>
							<PlusIcon
								className="size-4"
								aria-hidden="true"
							/>
							Add customer
						</Button>
					</CardHeader>
					<CardContent>
						{clients.length === 0 ? (
							<p className="py-8 text-center text-sm text-muted-foreground">
								No customers configured. Add a customer before
								creating a work record.
							</p>
						) : (
							<Table>
								<TableHeader>
									<TableRow>
										<TableHead>Customer</TableHead>
										<TableHead>Contact</TableHead>
										<TableHead>Address</TableHead>
										<TableHead className="text-right">
											Actions
										</TableHead>
									</TableRow>
								</TableHeader>
								<TableBody>
									{clients.map((client) => (
										<TableRow key={client.id}>
											<TableCell className="font-medium">
												{client.name}
											</TableCell>
											<TableCell>
												{client.contactName ||
													client.email}
												<span className="block text-xs text-muted-foreground">
													{client.email}
												</span>
											</TableCell>
											<TableCell>
												{client.city}, {client.postcode}
											</TableCell>
											<TableCell>
												<div className="flex justify-end gap-1">
													<Button
														type="button"
														variant="ghost"
														size="icon"
														aria-label={`Edit ${client.name}`}
														onClick={() =>
															openClient(client)
														}
													>
														<PencilIcon className="size-4" />
													</Button>
													<Button
														type="button"
														variant="ghost"
														size="icon"
														aria-label={`Delete ${client.name}`}
														onClick={() =>
															removeClient(client)
														}
													>
														<Trash2Icon className="size-4" />
													</Button>
												</div>
											</TableCell>
										</TableRow>
									))}
								</TableBody>
							</Table>
						)}
					</CardContent>
				</Card>
			</div>

			<Dialog
				open={clientDialogOpen}
				onOpenChange={setClientDialogOpen}
			>
				<DialogContent className="max-h-[90svh] overflow-y-auto sm:max-w-2xl">
					<form
						onSubmit={clientForm.handleSubmit(submitClient)}
						noValidate
					>
						<DialogHeader>
							<DialogTitle>
								{editingClient
									? 'Edit customer'
									: 'Add customer'}
							</DialogTitle>
							<DialogDescription>
								Enter the contact and postal details that should
								appear on invoices.
							</DialogDescription>
						</DialogHeader>
						<div className="grid gap-5 py-6 sm:grid-cols-2">
							<FormInput
								id="client-name"
								label="Customer name"
								description="Person or organisation that will receive the invoice."
								registration={clientForm.register('name')}
								error={
									clientForm.formState.errors.name?.message
								}
							/>
							<FormInput
								id="client-contact-name"
								label="Contact name (optional)"
								description="Main person to contact about invoices."
								registration={clientForm.register(
									'contactName',
								)}
							/>
							<FormInput
								id="client-email"
								label="Email"
								description="Address used to send invoices and billing queries."
								type="email"
								registration={clientForm.register('email')}
								error={
									clientForm.formState.errors.email?.message
								}
							/>
							<FormInput
								id="client-phone"
								label="Phone (optional)"
								description="Contact number for billing queries."
								type="tel"
								registration={clientForm.register('phone')}
							/>
							<FormInput
								id="client-address-line-1"
								label="Address line 1"
								description="Primary billing address for this customer."
								registration={clientForm.register(
									'addressLine1',
								)}
								error={
									clientForm.formState.errors.addressLine1
										?.message
								}
							/>
							<FormInput
								id="client-address-line-2"
								label="Address line 2 (optional)"
								description="Additional billing address details."
								registration={clientForm.register(
									'addressLine2',
								)}
							/>
							<FormInput
								id="client-city"
								label="Town or city"
								description="Town or city for the billing address."
								registration={clientForm.register('city')}
								error={
									clientForm.formState.errors.city?.message
								}
							/>
							<FormInput
								id="client-county"
								label="County (optional)"
								description="County for the billing address, if applicable."
								registration={clientForm.register('county')}
							/>
							<FormInput
								id="client-postcode"
								label="Postcode"
								description="Postcode for the billing address."
								registration={clientForm.register('postcode')}
								error={
									clientForm.formState.errors.postcode
										?.message
								}
							/>
						</div>
						{clientError && (
							<p
								role="alert"
								className="mb-4 text-sm text-destructive"
							>
								{clientError}
							</p>
						)}
						<DialogFooter>
							<Button
								type="button"
								variant="outline"
								onClick={() => setClientDialogOpen(false)}
							>
								Cancel
							</Button>
							<Button
								type="submit"
								disabled={clientForm.formState.isSubmitting}
							>
								{clientForm.formState.isSubmitting
									? 'Saving…'
									: 'Save customer'}
							</Button>
						</DialogFooter>
					</form>
				</DialogContent>
			</Dialog>
		</section>
	);
}

type Registration = ReturnType<ReturnType<typeof useForm>['register']>;

function FormInput({
	id,
	label,
	description,
	registration,
	error,
	type = 'text',
}: {
	id?: string;
	label: string;
	description?: string;
	registration: Registration;
	error?: string;
	type?: string;
}) {
	const inputId = id ?? registration.name;
	const descriptionId = `${inputId}-description`;
	const errorId = `${inputId}-error`;
	return (
		<div className="grid gap-2">
			<Label htmlFor={inputId}>{label}</Label>
			{description && (
				<FieldDescription id={descriptionId}>
					{description}
				</FieldDescription>
			)}
			<Input
				id={inputId}
				type={type}
				{...registration}
				aria-invalid={Boolean(error)}
				aria-describedby={
					[description && descriptionId, error && errorId]
						.filter(Boolean)
						.join(' ') || undefined
				}
			/>
			{error && (
				<p
					id={errorId}
					className="text-sm text-destructive"
				>
					{error}
				</p>
			)}
		</div>
	);
}

function FormSelect({
	label,
	description,
	name,
	value,
	onChange,
	onBlur,
	options,
}: {
	label: string;
	description?: string;
	name: string;
	value: BusinessProfileDto['businessType'];
	onChange: (value: BusinessProfileDto['businessType']) => void;
	onBlur: () => void;
	options: ReadonlyArray<
		readonly [BusinessProfileDto['businessType'], string]
	>;
}) {
	const descriptionId = `${name}-description`;
	const items = options.map(([optionValue, optionLabel]) => ({
		label: optionLabel,
		value: optionValue,
	}));
	return (
		<div className="grid gap-2">
			<Label htmlFor={name}>{label}</Label>
			{description && (
				<FieldDescription id={descriptionId}>
					{description}
				</FieldDescription>
			)}
			<Select
				items={items}
				name={name}
				value={value}
				onValueChange={(nextValue) => nextValue && onChange(nextValue)}
			>
				<SelectTrigger
					id={name}
					className="w-full"
					onBlur={onBlur}
					aria-required="true"
					aria-describedby={description ? descriptionId : undefined}
				>
					<SelectValue />
				</SelectTrigger>
				<SelectContent>
					{options.map(([optionValue, optionLabel]) => (
						<SelectItem
							key={optionValue}
							value={optionValue}
						>
							{optionLabel}
						</SelectItem>
					))}
				</SelectContent>
			</Select>
		</div>
	);
}
