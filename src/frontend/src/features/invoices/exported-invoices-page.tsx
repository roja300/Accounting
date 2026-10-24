import {
	BanknoteIcon,
	CalendarDaysIcon,
	CheckCircle2Icon,
	ChevronLeftIcon,
	ChevronRightIcon,
	DownloadIcon,
	FileOutputIcon,
	FileTextIcon,
	MailCheckIcon,
	SearchIcon,
	Trash2Icon,
	XIcon,
} from 'lucide-react';
import { type FormEvent, type ReactNode, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { Button, buttonVariants } from '@/components/ui/button';
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
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from '@/components/ui/select';
import { ExportInvoiceDialog } from '@/features/invoices/export-invoice-dialog';
import {
	deleteGeneratedInvoice,
	type GeneratedInvoiceDto,
	type GeneratedInvoicePageDto,
	generatedInvoicePdfUrl,
	getGeneratedInvoices,
	type InvoiceStatus,
	updateGeneratedInvoiceStatus,
} from '@/features/invoices/exported-invoice-api';
import { cn } from '@/lib/utils';

const PAGE_SIZE = 9;
const emptyFilters = { search: '', dateAfter: '', dateBefore: '', status: '' };
type Filters = typeof emptyFilters;

const statusDetails: Record<
	InvoiceStatus,
	{ label: string; className: string }
> = {
	exported: {
		label: 'Exported',
		className: 'bg-blue-500/10 text-blue-700 dark:text-blue-300',
	},
	sent: {
		label: 'Sent',
		className: 'bg-amber-500/10 text-amber-700 dark:text-amber-300',
	},
	paymentReceived: {
		label: 'Payment received',
		className: 'bg-emerald-500/10 text-emerald-700 dark:text-emerald-300',
	},
	deleted: {
		label: 'Deleted',
		className: 'bg-destructive/10 text-destructive',
	},
};

const dateFormatter = new Intl.DateTimeFormat('en-GB', {
	day: 'numeric',
	month: 'short',
	year: 'numeric',
});

function money(invoice: GeneratedInvoiceDto) {
	return new Intl.NumberFormat('en-GB', {
		style: 'currency',
		currency: invoice.currency,
	}).format(invoice.subtotal);
}

function InvoiceCard({
	invoice,
	onStatus,
	onDelete,
	busy,
}: {
	invoice: GeneratedInvoiceDto;
	onStatus: (invoice: GeneratedInvoiceDto, status: InvoiceStatus) => void;
	onDelete: (invoice: GeneratedInvoiceDto) => void;
	busy: boolean;
}) {
	const status = statusDetails[invoice.status];
	const deleted = invoice.status === 'deleted';
	return (
		<Card
			className={cn(
				'h-full shadow-sm transition-shadow hover:shadow-md',
				deleted && 'opacity-65',
			)}
			role="article"
			aria-labelledby={`invoice-${invoice.id}`}
		>
			<CardHeader className="border-b">
				<div className="flex items-start justify-between gap-3">
					<div>
						<CardTitle
							id={`invoice-${invoice.id}`}
							className="font-mono text-base"
						>
							{invoice.invoiceNumber}
						</CardTitle>
						<CardDescription className="mt-1">
							{invoice.clientName}
						</CardDescription>
					</div>
					<span
						className={cn(
							'shrink-0 rounded-full px-2.5 py-1 text-xs font-medium',
							status.className,
						)}
					>
						{status.label}
					</span>
				</div>
			</CardHeader>
			<CardContent className="flex flex-1 flex-col gap-4">
				<p className="text-2xl font-semibold tracking-tight">
					{money(invoice)}
				</p>
				<div className="grid gap-2 text-sm text-muted-foreground">
					<p className="flex items-center gap-2">
						<CalendarDaysIcon
							className="size-4"
							aria-hidden="true"
						/>
						Issued{' '}
						{dateFormatter.format(
							new Date(`${invoice.issueDate}T00:00:00`),
						)}
					</p>
					<p className="flex items-center gap-2">
						<BanknoteIcon
							className="size-4"
							aria-hidden="true"
						/>
						Due{' '}
						{dateFormatter.format(
							new Date(`${invoice.dueDate}T00:00:00`),
						)}
					</p>
					<p className="flex items-center gap-2">
						<FileTextIcon
							className="size-4"
							aria-hidden="true"
						/>
						{invoice.lines.length} work{' '}
						{invoice.lines.length === 1 ? 'record' : 'records'}
					</p>
				</div>
			</CardContent>
			<CardFooter className="flex-wrap gap-2 border-t">
				<a
					href={generatedInvoicePdfUrl(invoice.id)}
					aria-label={`Download ${invoice.invoiceNumber} PDF`}
					target="_blank"
					rel="noreferrer"
					className={buttonVariants({
						variant: 'outline',
						size: 'sm',
					})}
				>
					<DownloadIcon aria-hidden="true" />
					PDF
				</a>
				{!deleted && invoice.status === 'exported' && (
					<Button
						variant="outline"
						size="sm"
						onClick={() => onStatus(invoice, 'sent')}
						disabled={busy}
					>
						<MailCheckIcon aria-hidden="true" />
						Mark sent
					</Button>
				)}
				{!deleted && invoice.status !== 'paymentReceived' && (
					<Button
						variant="outline"
						size="sm"
						onClick={() => onStatus(invoice, 'paymentReceived')}
						disabled={busy}
					>
						<CheckCircle2Icon aria-hidden="true" />
						Payment received
					</Button>
				)}
				{!deleted && (
					<Button
						variant="ghost"
						size="sm"
						className="ml-auto text-destructive hover:text-destructive"
						onClick={() => onDelete(invoice)}
						disabled={busy}
						aria-label={`Delete ${invoice.invoiceNumber}`}
					>
						<Trash2Icon aria-hidden="true" />
					</Button>
				)}
			</CardFooter>
		</Card>
	);
}

function invoiceSummary(loading: boolean, data?: GeneratedInvoicePageDto) {
	if (loading) return 'Loading invoices...';
	if (!data?.totalItems) return '';
	return `${data.totalItems} invoice${data.totalItems === 1 ? '' : 's'}`;
}

function EmptyInvoiceState({ hasFilters }: { hasFilters: boolean }) {
	return (
		<Card className="min-h-64 w-full justify-center py-12 text-center shadow-sm">
			<CardHeader className="mx-auto w-full max-w-xl">
				<CardTitle>
					{hasFilters
						? 'No matching invoices'
						: 'No exported invoices yet'}
				</CardTitle>
				<CardDescription>
					{hasFilters
						? 'Try changing or clearing your filters.'
						: 'Generate an invoice from your ready work records.'}
				</CardDescription>
			</CardHeader>
			<CardContent className="flex justify-center">
				<Button
					render={<Link to="/invoices" />}
					nativeButton={false}
				>
					{hasFilters
						? 'View work records'
						: 'Generate your first invoice'}
				</Button>
			</CardContent>
		</Card>
	);
}

function InvoiceResults({
	data,
	loading,
	error,
	hasFilters,
	busyId,
	page,
	onStatus,
	onDelete,
	onPrevious,
	onNext,
}: {
	data?: GeneratedInvoicePageDto;
	loading: boolean;
	error: string;
	hasFilters: boolean;
	busyId: string;
	page: number;
	onStatus: (invoice: GeneratedInvoiceDto, status: InvoiceStatus) => void;
	onDelete: (invoice: GeneratedInvoiceDto) => void;
	onPrevious: () => void;
	onNext: () => void;
}) {
	let content: ReactNode;
	if (loading) {
		content = (
			<div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
				{['one', 'two', 'three', 'four', 'five', 'six'].map((key) => (
					<Card
						key={key}
						className="h-72 animate-pulse"
						aria-hidden="true"
					/>
				))}
			</div>
		);
	} else if (data && data.items.length > 0) {
		content = (
			<div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
				{data.items.map((invoice) => (
					<InvoiceCard
						key={invoice.id}
						invoice={invoice}
						onStatus={onStatus}
						onDelete={onDelete}
						busy={busyId === invoice.id}
					/>
				))}
			</div>
		);
	} else {
		content = <EmptyInvoiceState hasFilters={hasFilters} />;
	}

	return (
		<>
			<div
				className="mb-4 min-h-6 text-sm text-muted-foreground"
				aria-live="polite"
			>
				{invoiceSummary(loading, data)}
			</div>
			{error && (
				<Card
					className="mb-5 p-5 text-sm text-destructive"
					role="alert"
				>
					{error}
				</Card>
			)}
			{content}
			{data && data.totalItems > 0 && (
				<nav
					aria-label="Invoice pagination"
					className="mt-6 flex items-center justify-between border-t pt-5"
				>
					<Button
						variant="outline"
						onClick={onPrevious}
						disabled={page <= 1}
					>
						<ChevronLeftIcon aria-hidden="true" />
						Previous
					</Button>
					<p className="text-sm text-muted-foreground">
						Page {page} of {Math.max(data.totalPages, 1)}
					</p>
					<Button
						variant="outline"
						onClick={onNext}
						disabled={page >= data.totalPages}
					>
						Next
						<ChevronRightIcon aria-hidden="true" />
					</Button>
				</nav>
			)}
		</>
	);
}

export function ExportedInvoicesPage() {
	const [draft, setDraft] = useState<Filters>(emptyFilters);
	const [filters, setFilters] = useState<Filters>(emptyFilters);
	const [data, setData] = useState<GeneratedInvoicePageDto>();
	const [page, setPage] = useState(1);
	const [refresh, setRefresh] = useState(0);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState('');
	const [busyId, setBusyId] = useState('');
	const [deleteTarget, setDeleteTarget] = useState<GeneratedInvoiceDto>();
	const [exportOpen, setExportOpen] = useState(false);

	useEffect(() => {
		const controller = new AbortController();
		setLoading(true);
		setError('');
		getGeneratedInvoices(
			{ ...filters, page, pageSize: PAGE_SIZE },
			controller.signal,
		)
			.then(setData)
			.catch((requestError: unknown) => {
				if (
					!(
						requestError instanceof DOMException &&
						requestError.name === 'AbortError'
					)
				)
					setError('Invoices could not be loaded.');
			})
			.finally(() => {
				if (!controller.signal.aborted) setLoading(false);
			});
		return () => controller.abort();
	}, [filters, page, refresh]);

	function apply(event: FormEvent<HTMLFormElement>) {
		event.preventDefault();
		setPage(1);
		setFilters({ ...draft });
	}

	function clear() {
		setDraft(emptyFilters);
		setFilters(emptyFilters);
		setPage(1);
	}

	async function changeStatus(
		invoice: GeneratedInvoiceDto,
		status: InvoiceStatus,
	) {
		setBusyId(invoice.id);
		setError('');
		try {
			await updateGeneratedInvoiceStatus(invoice.id, status);
			setRefresh((value) => value + 1);
		} catch {
			setError('The invoice status could not be updated.');
		} finally {
			setBusyId('');
		}
	}

	async function confirmDelete() {
		if (!deleteTarget) return;
		setBusyId(deleteTarget.id);
		try {
			await deleteGeneratedInvoice(deleteTarget.id);
			setDeleteTarget(undefined);
			setRefresh((value) => value + 1);
		} catch {
			setError('The invoice could not be deleted.');
		} finally {
			setBusyId('');
		}
	}

	const hasFilters = Object.values(filters).some(Boolean);
	return (
		<section
			aria-label="Exported invoices page"
			className="min-h-[calc(100svh-4rem)] bg-muted/30 px-4 py-10 sm:px-6 lg:px-8"
		>
			<div className="mx-auto w-full max-w-6xl">
				<div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
					<div>
						<p className="mb-2 text-sm font-medium text-muted-foreground">
							Invoices
						</p>
						<h1 className="text-3xl font-semibold tracking-tight">
							Exported invoices
						</h1>
						<p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
							Track issued invoices, download their PDFs, and
							record when they are sent or paid.
						</p>
					</div>
					<div className="flex flex-wrap gap-2">
						<Button
							variant="outline"
							render={<Link to="/invoices" />}
							nativeButton={false}
						>
							View work records
						</Button>
						<Button onClick={() => setExportOpen(true)}>
							<FileOutputIcon aria-hidden="true" />
							Generate invoice
						</Button>
					</div>
				</div>

				<Card className="mb-6 shadow-sm">
					<CardHeader className="border-b">
						<CardTitle>Filter invoices</CardTitle>
						<CardDescription>
							Search by invoice number or client, then narrow by
							issue date or status.
						</CardDescription>
					</CardHeader>
					<CardContent>
						<form
							className="grid gap-4 md:grid-cols-2 lg:grid-cols-5"
							onSubmit={apply}
						>
							<div className="space-y-2 md:col-span-2">
								<Label htmlFor="invoice-search">Search</Label>
								<div className="relative">
									<SearchIcon
										className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
										aria-hidden="true"
									/>
									<Input
										id="invoice-search"
										className="pl-8"
										value={draft.search}
										onChange={(event) =>
											setDraft((value) => ({
												...value,
												search: event.target.value,
											}))
										}
										placeholder="Invoice number or client"
									/>
								</div>
							</div>
							<div className="space-y-2">
								<Label htmlFor="invoice-date-after">
									Issued after
								</Label>
								<Input
									id="invoice-date-after"
									type="date"
									value={draft.dateAfter}
									max={draft.dateBefore || undefined}
									onChange={(event) =>
										setDraft((value) => ({
											...value,
											dateAfter: event.target.value,
										}))
									}
								/>
							</div>
							<div className="space-y-2">
								<Label htmlFor="invoice-date-before">
									Issued before
								</Label>
								<Input
									id="invoice-date-before"
									type="date"
									value={draft.dateBefore}
									min={draft.dateAfter || undefined}
									onChange={(event) =>
										setDraft((value) => ({
											...value,
											dateBefore: event.target.value,
										}))
									}
								/>
							</div>
							<div className="space-y-2">
								<Label htmlFor="invoice-status">Status</Label>
								<Select
									value={draft.status || 'all'}
									onValueChange={(value) =>
										setDraft((current) => ({
											...current,
											status:
												value === 'all'
													? ''
													: (value ?? ''),
										}))
									}
								>
									<SelectTrigger
										id="invoice-status"
										className="w-full"
									>
										<SelectValue />
									</SelectTrigger>
									<SelectContent
										align="start"
										alignItemWithTrigger={false}
									>
										<SelectItem value="all">
											All statuses
										</SelectItem>
										{Object.entries(statusDetails).map(
											([value, details]) => (
												<SelectItem
													key={value}
													value={value}
												>
													{details.label}
												</SelectItem>
											),
										)}
									</SelectContent>
								</Select>
							</div>
							<div className="flex items-end justify-end gap-2 md:col-span-2 lg:col-span-5">
								<Button
									type="button"
									variant="ghost"
									onClick={clear}
									disabled={
										!Object.values(draft).some(Boolean)
									}
								>
									<XIcon aria-hidden="true" />
									Clear
								</Button>
								<Button type="submit">Apply filters</Button>
							</div>
						</form>
					</CardContent>
				</Card>

				<InvoiceResults
					data={data}
					loading={loading}
					error={error}
					hasFilters={hasFilters}
					busyId={busyId}
					page={page}
					onStatus={changeStatus}
					onDelete={setDeleteTarget}
					onPrevious={() => setPage((value) => value - 1)}
					onNext={() => setPage((value) => value + 1)}
				/>
			</div>

			<ExportInvoiceDialog
				open={exportOpen}
				onOpenChange={setExportOpen}
				onGenerated={() => setRefresh((value) => value + 1)}
			/>

			<Dialog
				open={Boolean(deleteTarget)}
				onOpenChange={(open) => {
					if (!(open || busyId)) setDeleteTarget(undefined);
				}}
			>
				<DialogContent>
					<DialogHeader>
						<DialogTitle>Delete this invoice?</DialogTitle>
						<DialogDescription>
							{deleteTarget?.invoiceNumber} will be marked as
							deleted and kept in your records. This action cannot
							be undone.
						</DialogDescription>
					</DialogHeader>
					<DialogFooter>
						<Button
							variant="outline"
							onClick={() => setDeleteTarget(undefined)}
							disabled={Boolean(busyId)}
						>
							Cancel
						</Button>
						<Button
							variant="destructive"
							onClick={confirmDelete}
							disabled={Boolean(busyId)}
						>
							<Trash2Icon aria-hidden="true" />
							{busyId ? 'Deleting…' : 'Delete invoice'}
						</Button>
					</DialogFooter>
				</DialogContent>
			</Dialog>
		</section>
	);
}
