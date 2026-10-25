import {
	ChevronLeftIcon,
	ChevronRightIcon,
	FileUpIcon,
	SearchIcon,
	Trash2Icon,
	XIcon,
} from 'lucide-react';
import {
	type FormEvent,
	useEffect,
	useId,
	useMemo,
	useRef,
	useState,
} from 'react';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
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
	deletePayments,
	getPaymentImports,
	getPayments,
	importPayments,
	PaymentApiError,
	type PaymentDto,
	type PaymentFilters,
	type PaymentImportDto,
	type PaymentPageDto,
} from '@/features/payments/payment-api';

const PAGE_SIZE = 25;
const emptyFilters = {
	dateAfter: '',
	dateBefore: '',
	search: '',
	direction: '' as const,
	importId: '',
};
type FilterValues = typeof emptyFilters;

const dateFormatter = new Intl.DateTimeFormat('en-GB', {
	dateStyle: 'medium',
	timeStyle: 'short',
});

function formatMoney(value: number, currency: string) {
	try {
		return new Intl.NumberFormat('en-GB', {
			style: 'currency',
			currency,
		}).format(value);
	} catch {
		return `${value.toFixed(2)} ${currency}`;
	}
}

function ImportDialog({
	open,
	onOpenChange,
	onImported,
}: {
	open: boolean;
	onOpenChange: (open: boolean) => void;
	onImported: (result: PaymentImportDto) => void;
}) {
	const inputId = useId();
	const inputRef = useRef<HTMLInputElement>(null);
	const [file, setFile] = useState<File>();
	const [submitting, setSubmitting] = useState(false);
	const [errors, setErrors] = useState<string[]>([]);

	function reset() {
		setFile(undefined);
		setErrors([]);
		if (inputRef.current) inputRef.current.value = '';
	}

	async function submit(event: FormEvent<HTMLFormElement>) {
		event.preventDefault();
		if (!file) {
			setErrors(['Select a CSV file to import.']);
			return;
		}
		setSubmitting(true);
		setErrors([]);
		try {
			const result = await importPayments(file);
			onImported(result);
			reset();
			onOpenChange(false);
		} catch (error) {
			const validationErrors =
				error instanceof PaymentApiError
					? Object.entries(error.errors).flatMap(([row, messages]) =>
							messages.map((message) =>
								row.startsWith('rows[')
									? `${row}: ${message}`
									: message,
							),
						)
					: [];
			setErrors(
				validationErrors.length > 0
					? validationErrors
					: ['The file could not be imported. Please try again.'],
			);
		} finally {
			setSubmitting(false);
		}
	}

	return (
		<Dialog
			open={open}
			onOpenChange={(next) => {
				if (!submitting) {
					if (!next) reset();
					onOpenChange(next);
				}
			}}
		>
			<DialogContent>
				<form onSubmit={submit}>
					<DialogHeader>
						<DialogTitle>Import payment data</DialogTitle>
						<DialogDescription>
							Upload a bank CSV. The entire file is checked before
							any payments are saved.
						</DialogDescription>
					</DialogHeader>
					<div className="my-5 space-y-2">
						<Label htmlFor={inputId}>Bank CSV</Label>
						<Input
							ref={inputRef}
							id={inputId}
							type="file"
							accept=".csv,text/csv"
							onChange={(event) =>
								setFile(event.target.files?.[0])
							}
							disabled={submitting}
						/>
						<p className="text-xs text-muted-foreground">
							Maximum file size: 10 MB.
						</p>
					</div>
					{errors.length > 0 && (
						<div
							role="alert"
							className="mb-5 rounded-md border border-destructive/30 bg-destructive/5 p-3 text-sm text-destructive"
						>
							<p className="font-medium">
								The CSV could not be imported:
							</p>
							<ul className="mt-1 list-disc space-y-1 pl-5">
								{errors.map((error) => (
									<li key={error}>{error}</li>
								))}
							</ul>
						</div>
					)}
					<DialogFooter>
						<Button
							type="button"
							variant="outline"
							onClick={() => onOpenChange(false)}
							disabled={submitting}
						>
							Cancel
						</Button>
						<Button
							type="submit"
							disabled={submitting}
						>
							{submitting ? 'Importing…' : 'Import payments'}
						</Button>
					</DialogFooter>
				</form>
			</DialogContent>
		</Dialog>
	);
}

type DeleteMode = 'import' | 'dates' | 'all';

function DeleteDialog({
	open,
	onOpenChange,
	imports,
	onDeleted,
}: {
	open: boolean;
	onOpenChange: (open: boolean) => void;
	imports: PaymentImportDto[];
	onDeleted: () => void;
}) {
	const [mode, setMode] = useState<DeleteMode>('import');
	const [importId, setImportId] = useState('');
	const [dateAfter, setDateAfter] = useState('');
	const [dateBefore, setDateBefore] = useState('');
	const [submitting, setSubmitting] = useState(false);
	const [error, setError] = useState('');

	async function submit(event: FormEvent<HTMLFormElement>) {
		event.preventDefault();
		if (mode === 'import' && !importId) {
			setError('Select an imported file.');
			return;
		}
		if (mode === 'dates' && !(dateAfter || dateBefore)) {
			setError('Enter at least one date.');
			return;
		}
		setSubmitting(true);
		setError('');
		try {
			await deletePayments(
				mode === 'all'
					? { all: true }
					: mode === 'import'
						? { importId }
						: { dateAfter, dateBefore },
			);
			onDeleted();
			onOpenChange(false);
		} catch {
			setError(
				'The selected payments could not be deleted. Please try again.',
			);
		} finally {
			setSubmitting(false);
		}
	}

	return (
		<Dialog
			open={open}
			onOpenChange={(next) => !submitting && onOpenChange(next)}
		>
			<DialogContent>
				<form onSubmit={submit}>
					<DialogHeader>
						<DialogTitle>Delete imported payments</DialogTitle>
						<DialogDescription>
							This permanently removes payment data. Choose
							exactly what to delete.
						</DialogDescription>
					</DialogHeader>
					<div className="my-5 space-y-4">
						<div className="space-y-2">
							<Label htmlFor="delete-mode">Delete by</Label>
							<Select
								value={mode}
								onValueChange={(value) => {
									setMode((value ?? 'import') as DeleteMode);
									setError('');
								}}
							>
								<SelectTrigger
									id="delete-mode"
									className="w-full"
								>
									<SelectValue />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="import">
										Imported file
									</SelectItem>
									<SelectItem value="dates">
										Transaction date
									</SelectItem>
									<SelectItem value="all">
										All payments
									</SelectItem>
								</SelectContent>
							</Select>
						</div>
						{mode === 'import' && (
							<div className="space-y-2">
								<Label htmlFor="delete-import">
									Imported file
								</Label>
								<Select
									value={importId}
									onValueChange={(value) =>
										setImportId(value ?? '')
									}
								>
									<SelectTrigger
										id="delete-import"
										className="w-full"
									>
										<SelectValue placeholder="Select a file" />
									</SelectTrigger>
									<SelectContent>
										{imports.map((item) => (
											<SelectItem
												key={item.id}
												value={item.id}
											>
												{item.fileName} ({item.rowCount}
												)
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>
						)}
						{mode === 'dates' && (
							<div className="grid gap-3 sm:grid-cols-2">
								<div className="space-y-2">
									<Label htmlFor="delete-date-after">
										From date
									</Label>
									<Input
										id="delete-date-after"
										type="date"
										value={dateAfter}
										max={dateBefore || undefined}
										onChange={(event) =>
											setDateAfter(event.target.value)
										}
									/>
								</div>
								<div className="space-y-2">
									<Label htmlFor="delete-date-before">
										To date
									</Label>
									<Input
										id="delete-date-before"
										type="date"
										value={dateBefore}
										min={dateAfter || undefined}
										onChange={(event) =>
											setDateBefore(event.target.value)
										}
									/>
								</div>
							</div>
						)}
						{mode === 'all' && (
							<p className="rounded-md border border-destructive/30 bg-destructive/5 p-3 text-sm text-destructive">
								Every imported payment and import record will be
								permanently deleted.
							</p>
						)}
					</div>
					{error && (
						<p
							role="alert"
							className="mb-4 text-sm text-destructive"
						>
							{error}
						</p>
					)}
					<DialogFooter>
						<Button
							type="button"
							variant="outline"
							onClick={() => onOpenChange(false)}
							disabled={submitting}
						>
							Cancel
						</Button>
						<Button
							type="submit"
							variant="destructive"
							disabled={submitting}
						>
							{submitting ? 'Deleting…' : 'Delete payments'}
						</Button>
					</DialogFooter>
				</form>
			</DialogContent>
		</Dialog>
	);
}

function FilterPanel({
	filters,
	imports,
	onChange,
	onSubmit,
	onClear,
}: {
	filters: FilterValues;
	imports: PaymentImportDto[];
	onChange: (name: keyof FilterValues, value: string) => void;
	onSubmit: (event: FormEvent<HTMLFormElement>) => void;
	onClear: () => void;
}) {
	return (
		<Card className="mb-6 shadow-sm">
			<CardHeader className="border-b">
				<CardTitle>Filter payments</CardTitle>
			</CardHeader>
			<CardContent>
				<form
					onSubmit={onSubmit}
					className="grid gap-4 md:grid-cols-2 lg:grid-cols-6"
				>
					<div className="space-y-2 md:col-span-2">
						<Label htmlFor="payment-search">Search</Label>
						<div className="relative">
							<SearchIcon
								className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
								aria-hidden="true"
							/>
							<Input
								id="payment-search"
								className="pl-8"
								placeholder="Description, payer, or reference"
								value={filters.search}
								maxLength={200}
								onChange={(event) =>
									onChange('search', event.target.value)
								}
							/>
						</div>
					</div>
					<div className="space-y-2">
						<Label htmlFor="payment-date-after">From date</Label>
						<Input
							id="payment-date-after"
							type="date"
							value={filters.dateAfter}
							max={filters.dateBefore || undefined}
							onChange={(event) =>
								onChange('dateAfter', event.target.value)
							}
						/>
					</div>
					<div className="space-y-2">
						<Label htmlFor="payment-date-before">To date</Label>
						<Input
							id="payment-date-before"
							type="date"
							value={filters.dateBefore}
							min={filters.dateAfter || undefined}
							onChange={(event) =>
								onChange('dateBefore', event.target.value)
							}
						/>
					</div>
					<div className="space-y-2">
						<Label htmlFor="payment-direction">Direction</Label>
						<Select
							value={filters.direction || 'all'}
							onValueChange={(value) =>
								onChange(
									'direction',
									value === 'all' ? '' : (value ?? ''),
								)
							}
						>
							<SelectTrigger
								id="payment-direction"
								className="w-full"
							>
								<SelectValue />
							</SelectTrigger>
							<SelectContent>
								<SelectItem value="all">
									All directions
								</SelectItem>
								<SelectItem value="incoming">
									Money in
								</SelectItem>
								<SelectItem value="outgoing">
									Money out
								</SelectItem>
							</SelectContent>
						</Select>
					</div>
					<div className="space-y-2">
						<Label htmlFor="payment-import">Imported file</Label>
						<Select
							value={filters.importId || 'all'}
							onValueChange={(value) =>
								onChange(
									'importId',
									value === 'all' ? '' : (value ?? ''),
								)
							}
						>
							<SelectTrigger
								id="payment-import"
								className="w-full"
							>
								<SelectValue />
							</SelectTrigger>
							<SelectContent>
								<SelectItem value="all">All files</SelectItem>
								{imports.map((item) => (
									<SelectItem
										key={item.id}
										value={item.id}
									>
										{item.fileName}
									</SelectItem>
								))}
							</SelectContent>
						</Select>
					</div>
					<div className="flex items-end justify-end gap-2 md:col-span-2 lg:col-span-6">
						<Button
							type="button"
							variant="ghost"
							onClick={onClear}
							disabled={!Object.values(filters).some(Boolean)}
						>
							<XIcon aria-hidden="true" />
							Clear
						</Button>
						<Button type="submit">Apply filters</Button>
					</div>
				</form>
			</CardContent>
		</Card>
	);
}

function PaymentRows({ payments }: { payments: PaymentDto[] }) {
	return payments.map((payment) => (
		<TableRow key={payment.id}>
			<TableCell className="font-medium">
				{dateFormatter.format(new Date(payment.transactionAt))}
			</TableCell>
			<TableCell className="max-w-80 whitespace-normal">
				<span className="font-medium">{payment.description}</span>
				{payment.reference && (
					<span className="mt-0.5 block text-xs text-muted-foreground">
						{payment.reference}
					</span>
				)}
			</TableCell>
			<TableCell>
				{payment.counterparty || (
					<span className="text-muted-foreground">—</span>
				)}
			</TableCell>
			<TableCell
				className={
					payment.amount < 0
						? 'text-right tabular-nums text-destructive'
						: 'text-right tabular-nums text-foreground'
				}
			>
				{formatMoney(payment.amount, payment.currency)}
			</TableCell>
			<TableCell className="text-right tabular-nums">
				{payment.balance === null
					? '—'
					: formatMoney(payment.balance, payment.currency)}
			</TableCell>
			<TableCell
				className="max-w-48 truncate text-muted-foreground"
				title={payment.importFileName}
			>
				{payment.importFileName}
			</TableCell>
		</TableRow>
	));
}

export function PaymentsPage() {
	const [draftFilters, setDraftFilters] =
		useState<FilterValues>(emptyFilters);
	const [appliedFilters, setAppliedFilters] =
		useState<FilterValues>(emptyFilters);
	const [payments, setPayments] = useState<PaymentPageDto>();
	const [imports, setImports] = useState<PaymentImportDto[]>([]);
	const [page, setPage] = useState(1);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState(false);
	const [refresh, setRefresh] = useState(0);
	const [importOpen, setImportOpen] = useState(false);
	const [deleteOpen, setDeleteOpen] = useState(false);
	const [status, setStatus] = useState('');

	useEffect(() => {
		const controller = new AbortController();
		getPaymentImports(controller.signal)
			.then(setImports)
			.catch(() => undefined);
		return () => controller.abort();
	}, [refresh]);

	useEffect(() => {
		const controller = new AbortController();
		setLoading(true);
		setError(false);
		const filters: PaymentFilters = {
			...appliedFilters,
			direction: appliedFilters.direction,
			page,
			pageSize: PAGE_SIZE,
		};
		getPayments(filters, controller.signal)
			.then(setPayments)
			.catch((requestError: unknown) => {
				if (
					!(
						requestError instanceof DOMException &&
						requestError.name === 'AbortError'
					)
				)
					setError(true);
			})
			.finally(() => {
				if (!controller.signal.aborted) setLoading(false);
			});
		return () => controller.abort();
	}, [appliedFilters, page, refresh]);

	const summary = useMemo(() => {
		if (!payments || payments.totalItems === 0) return 'No payments';
		const first = (payments.page - 1) * payments.pageSize + 1;
		return `${first}–${Math.min(payments.page * payments.pageSize, payments.totalItems)} of ${payments.totalItems} payments`;
	}, [payments]);

	function completed(message: string) {
		setPage(1);
		setStatus(message);
		setRefresh((value) => value + 1);
	}

	return (
		<section
			aria-labelledby="payments-title"
			className="min-h-[calc(100svh-4rem)] bg-muted/30 px-4 py-10 sm:px-6 lg:px-8"
		>
			<div className="mx-auto w-full max-w-7xl">
				<div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
					<div>
						<p className="mb-2 text-sm font-medium text-muted-foreground">
							Payments
						</p>
						<h1
							id="payments-title"
							className="text-3xl font-semibold tracking-tight"
						>
							Payment activity
						</h1>
						<p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
							Import bank transactions, find income and expenses,
							and keep source details for matching work and tax
							records.
						</p>
					</div>
					<div className="flex flex-wrap gap-2">
						<Button
							variant="outline"
							size="lg"
							onClick={() => setDeleteOpen(true)}
							disabled={imports.length === 0}
						>
							<Trash2Icon aria-hidden="true" />
							Delete payments
						</Button>
						<Button
							size="lg"
							onClick={() => setImportOpen(true)}
						>
							<FileUpIcon aria-hidden="true" />
							Import CSV
						</Button>
					</div>
				</div>
				<FilterPanel
					filters={draftFilters}
					imports={imports}
					onChange={(name, value) =>
						setDraftFilters((current) => ({
							...current,
							[name]: value,
						}))
					}
					onSubmit={(event) => {
						event.preventDefault();
						setPage(1);
						setAppliedFilters({ ...draftFilters });
					}}
					onClear={() => {
						setDraftFilters(emptyFilters);
						setAppliedFilters(emptyFilters);
						setPage(1);
					}}
				/>
				<div
					aria-live="polite"
					className="mb-4 flex min-h-6 items-center justify-between text-sm text-muted-foreground"
				>
					<p>{error ? '' : summary}</p>
					{loading && <p>Loading payments…</p>}
				</div>
				{status && (
					<p
						role="status"
						className="mb-4 text-sm text-muted-foreground"
					>
						{status}
					</p>
				)}
				<Card
					className="overflow-hidden p-0 shadow-sm"
					aria-busy={loading}
				>
					<Table>
						<TableHeader>
							<TableRow>
								<TableHead>Date and time</TableHead>
								<TableHead>Description</TableHead>
								<TableHead>From / to</TableHead>
								<TableHead className="text-right">
									Amount
								</TableHead>
								<TableHead className="text-right">
									Balance
								</TableHead>
								<TableHead>Imported file</TableHead>
							</TableRow>
						</TableHeader>
						<TableBody>
							{!(loading || error) &&
								payments &&
								payments.items.length > 0 && (
									<PaymentRows payments={payments.items} />
								)}
							{!(loading || error) &&
								(!payments || payments.items.length === 0) && (
									<TableRow>
										<TableCell
											colSpan={6}
											className="h-40 text-center text-muted-foreground"
										>
											{Object.values(appliedFilters).some(
												Boolean,
											)
												? 'No payments match these filters.'
												: 'No payments imported yet.'}
										</TableCell>
									</TableRow>
								)}
							{!loading && error && (
								<TableRow>
									<TableCell
										colSpan={6}
										className="h-40 text-center"
									>
										<div role="alert">
											<p className="font-medium">
												Payments could not be loaded
											</p>
											<p className="mt-1 text-muted-foreground">
												Check your connection and try
												again.
											</p>
											<Button
												className="mt-4"
												variant="outline"
												onClick={() =>
													setRefresh(
														(value) => value + 1,
													)
												}
											>
												Try again
											</Button>
										</div>
									</TableCell>
								</TableRow>
							)}
						</TableBody>
					</Table>
				</Card>
				{!(loading || error) && payments && payments.totalPages > 1 && (
					<nav
						aria-label="Payment pagination"
						className="mt-6 flex items-center justify-between"
					>
						<Button
							variant="outline"
							onClick={() => setPage((value) => value - 1)}
							disabled={page <= 1}
						>
							<ChevronLeftIcon aria-hidden="true" />
							Previous
						</Button>
						<p className="text-sm text-muted-foreground">
							Page {payments.page} of {payments.totalPages}
						</p>
						<Button
							variant="outline"
							onClick={() => setPage((value) => value + 1)}
							disabled={page >= payments.totalPages}
						>
							Next
							<ChevronRightIcon aria-hidden="true" />
						</Button>
					</nav>
				)}
			</div>
			<ImportDialog
				open={importOpen}
				onOpenChange={setImportOpen}
				onImported={(result) =>
					completed(
						`${result.rowCount} ${result.rowCount === 1 ? 'payment' : 'payments'} imported from ${result.fileName}.`,
					)
				}
			/>
			<DeleteDialog
				open={deleteOpen}
				onOpenChange={setDeleteOpen}
				imports={imports}
				onDeleted={() =>
					completed('The selected payments were deleted.')
				}
			/>
		</section>
	);
}
