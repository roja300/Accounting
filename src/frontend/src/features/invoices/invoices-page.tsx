import {
	Building2Icon,
	CalendarDaysIcon,
	ChevronLeftIcon,
	ChevronRightIcon,
	EllipsisVerticalIcon,
	FolderKanbanIcon,
	PencilIcon,
	PlusIcon,
	SearchIcon,
	Trash2Icon,
	XIcon,
} from 'lucide-react';
import { type FormEvent, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

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
	deleteWorkRecord,
	getWorkRecordOptions,
	getWorkRecords,
	type WorkRecordDto,
	type WorkRecordPageDto,
} from '@/features/invoices/invoice-api';

const PAGE_SIZE = 9;
const skeletonKeys = ['one', 'two', 'three', 'four', 'five', 'six'];
const dateFormatter = new Intl.DateTimeFormat('en-GB', {
	day: 'numeric',
	month: 'short',
	year: 'numeric',
});
const emptyFilters = {
	search: '',
	dateAfter: '',
	dateBefore: '',
	client: '',
	project: '',
};
type Filters = typeof emptyFilters;

function formatValue(record: WorkRecordDto) {
	return new Intl.NumberFormat('en-GB', {
		style: 'currency',
		currency: record.currency,
	}).format(record.value);
}

function WorkRecordCard({
	record,
	onDelete,
}: {
	record: WorkRecordDto;
	onDelete: (record: WorkRecordDto) => Promise<void>;
}) {
	const [confirmingDelete, setConfirmingDelete] = useState(false);
	const [deleting, setDeleting] = useState(false);

	async function confirmDelete() {
		setDeleting(true);
		try {
			await onDelete(record);
			setConfirmingDelete(false);
		} catch {
			// The page displays the request error while the dialog remains open.
		} finally {
			setDeleting(false);
		}
	}

	return (
		<>
			<Card
				className="relative h-full shadow-sm transition-shadow hover:shadow-md"
				role="article"
				aria-labelledby={`record-${record.id}`}
			>
				<CardHeader className="border-b">
				{record.isExported && (
					<span className="mb-1 w-fit rounded-full bg-emerald-500/10 px-2.5 py-1 text-xs font-medium text-emerald-700 dark:text-emerald-300">
						Exported
					</span>
				)}
					<CardTitle
						id={`record-${record.id}`}
						className="pr-2 text-base"
					>
						{record.description}
					</CardTitle>
					<CardDescription>
						Recorded{' '}
						{dateFormatter.format(new Date(record.createdAt))}
					</CardDescription>
					<div className="absolute top-4 right-4">
						<details className="relative">
							<summary
								className="flex size-8 cursor-pointer list-none items-center justify-center rounded-lg text-muted-foreground hover:bg-muted [&::-webkit-details-marker]:hidden"
								aria-label={`Actions for ${record.description}`}
							>
								<EllipsisVerticalIcon
									className="size-4"
									aria-hidden="true"
								/>
							</summary>
							<div
								className="absolute top-9 right-0 z-10 w-40 rounded-lg bg-popover p-1 text-popover-foreground shadow-md ring-1 ring-foreground/10"
								role="menu"
							>
								<Link
									to={`/work/${record.id}/edit`}
									role="menuitem"
								aria-disabled={record.isExported}
								onClick={(event) => {
									if (record.isExported)
										event.preventDefault();
								}}
								className={
									record.isExported
										? 'flex w-full cursor-not-allowed items-center gap-2 rounded px-2 py-1.5 text-left text-sm opacity-50'
										: 'flex w-full items-center gap-2 rounded px-2 py-1.5 text-left text-sm hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring'
								}
								>
									<PencilIcon aria-hidden="true" />
									Edit
								</Link>
								<button
									type="button"
									role="menuitem"
									onClick={() => setConfirmingDelete(true)}
									className="flex w-full items-center gap-2 rounded px-2 py-1.5 text-left text-sm text-destructive hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
								>
									<Trash2Icon aria-hidden="true" />
									Delete
								</button>
							</div>
						</details>
					</div>
				</CardHeader>
				<CardContent className="flex flex-1 flex-col gap-4">
					<div className="grid gap-3 text-sm text-muted-foreground">
						<div className="flex items-center gap-2">
							<Building2Icon
								className="size-4"
								aria-hidden="true"
							/>
							<span className="truncate">{record.client}</span>
						</div>
						<div className="flex items-center gap-2">
							<FolderKanbanIcon
								className="size-4"
								aria-hidden="true"
							/>
							<span className="truncate">{record.project}</span>
						</div>
						<div className="flex items-center gap-2">
							<CalendarDaysIcon
								className="size-4"
								aria-hidden="true"
							/>
							<span>
								{dateFormatter.format(
									new Date(record.createdAt),
								)}
							</span>
						</div>
					</div>
					{record.comments && (
						<p className="line-clamp-2 text-sm leading-6 text-muted-foreground">
							{record.comments}
						</p>
					)}
					<p className="mt-auto text-xl font-semibold tracking-tight">
						{formatValue(record)}
					</p>
				</CardContent>
			</Card>
			<Dialog
				open={confirmingDelete}
				onOpenChange={setConfirmingDelete}
			>
				<DialogContent>
					<DialogHeader>
						<DialogTitle>Delete work record?</DialogTitle>
						<DialogDescription>
							“{record.description}” will be hidden from your work
							records.
						</DialogDescription>
					</DialogHeader>
					<DialogFooter>
						<Button
							type="button"
							variant="outline"
							onClick={() => setConfirmingDelete(false)}
							disabled={deleting}
						>
							Cancel
						</Button>
						<Button
							type="button"
							variant="destructive"
							onClick={confirmDelete}
							disabled={deleting}
						>
							{deleting ? 'Deleting…' : 'Delete work record'}
						</Button>
					</DialogFooter>
				</DialogContent>
			</Dialog>
		</>
	);
}

function FilterPanel({
	filters,
	clients,
	projects,
	onChange,
	onSubmit,
	onClear,
}: {
	filters: Filters;
	clients: string[];
	projects: string[];
	onChange: (name: keyof Filters, value: string) => void;
	onSubmit: (event: FormEvent<HTMLFormElement>) => void;
	onClear: () => void;
}) {
	return (
		<Card className="mb-6 shadow-sm">
			<CardHeader className="border-b">
				<CardTitle>Filter work records</CardTitle>
				<CardDescription>
					Narrow the list by date, client, project, or description.
				</CardDescription>
			</CardHeader>
			<CardContent>
				<form
					onSubmit={onSubmit}
					className="grid gap-4 md:grid-cols-2 lg:grid-cols-6"
				>
					<div className="space-y-2 md:col-span-2 lg:col-span-2">
						<Label htmlFor="search-description">
							Search description
						</Label>
						<div className="relative">
							<SearchIcon
								className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
								aria-hidden="true"
							/>
							<Input
								id="search-description"
								value={filters.search}
								onChange={(event) =>
									onChange('search', event.target.value)
								}
								placeholder="e.g. bookkeeping"
								maxLength={200}
								className="pl-8"
							/>
						</div>
					</div>
					<div className="space-y-2">
						<Label htmlFor="date-after">Date after</Label>
						<Input
							id="date-after"
							type="date"
							value={filters.dateAfter}
							max={filters.dateBefore || undefined}
							onChange={(event) =>
								onChange('dateAfter', event.target.value)
							}
						/>
					</div>
					<div className="space-y-2">
						<Label htmlFor="date-before">Date before</Label>
						<Input
							id="date-before"
							type="date"
							value={filters.dateBefore}
							min={filters.dateAfter || undefined}
							onChange={(event) =>
								onChange('dateBefore', event.target.value)
							}
						/>
					</div>
					<FilterSelect
						id="client-filter"
						label="Client"
						value={filters.client}
						options={clients}
						allLabel="All clients"
						onChange={(value) => onChange('client', value)}
					/>
					<FilterSelect
						id="project-filter"
						label="Project"
						value={filters.project}
						options={projects}
						allLabel="All projects"
						onChange={(value) => onChange('project', value)}
					/>
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

function FilterSelect({
	id,
	label,
	value,
	options,
	allLabel,
	onChange,
}: {
	id: string;
	label: string;
	value: string;
	options: string[];
	allLabel: string;
	onChange: (value: string) => void;
}) {
	return (
		<div className="space-y-2">
			<Label htmlFor={id}>{label}</Label>
			<Select
				value={value || 'all'}
				onValueChange={(next) =>
					onChange(next === 'all' ? '' : (next ?? ''))
				}
			>
				<SelectTrigger
					id={id}
					className="w-full"
				>
					<SelectValue />
				</SelectTrigger>
				<SelectContent>
					<SelectItem value="all">{allLabel}</SelectItem>
					{options.map((option) => (
						<SelectItem
							key={option}
							value={option}
						>
							{option}
						</SelectItem>
					))}
				</SelectContent>
			</Select>
		</div>
	);
}

function Results({
	loading,
	error,
	records,
	hasFilters,
	onRetry,
	onClear,
	onDelete,
}: {
	loading: boolean;
	error: boolean;
	records?: WorkRecordPageDto;
	hasFilters: boolean;
	onRetry: () => void;
	onClear: () => void;
	onDelete: (record: WorkRecordDto) => Promise<void>;
}) {
	if (loading)
		return (
			<div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
				{skeletonKeys.map((key) => (
					<Card
						key={key}
						className="h-64 animate-pulse shadow-sm"
						aria-hidden="true"
					/>
				))}
			</div>
		);
	if (error)
		return (
			<Card
				className="items-center py-12 text-center shadow-sm"
				role="alert"
			>
				<CardHeader>
					<CardTitle>Work records could not be loaded</CardTitle>
					<CardDescription>
						Check your connection and try again.
					</CardDescription>
				</CardHeader>
				<CardContent>
					<Button onClick={onRetry}>Try again</Button>
				</CardContent>
			</Card>
		);
	if (!records || records.items.length === 0)
		return (
			<Card className="min-h-64 w-full justify-center py-12 text-center shadow-sm">
				<CardHeader className="mx-auto w-full max-w-xl">
					<CardTitle>
						{hasFilters
							? 'No matching work records'
							: 'No work records yet'}
					</CardTitle>
					<CardDescription>
						{hasFilters
							? 'Try changing or clearing your filters.'
							: 'Create a record when you finish work for a client.'}
					</CardDescription>
				</CardHeader>
				{hasFilters && (
					<CardContent className="flex justify-center">
						<Button
							variant="outline"
							onClick={onClear}
						>
							Clear filters
						</Button>
					</CardContent>
				)}
			</Card>
		);
	return (
		<div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
			{records.items.map((record) => (
				<WorkRecordCard
					key={record.id}
					record={record}
					onDelete={onDelete}
				/>
			))}
		</div>
	);
}

function Pagination({
	records,
	loading,
	error,
	onPrevious,
	onNext,
}: {
	records?: WorkRecordPageDto;
	loading: boolean;
	error: boolean;
	onPrevious: () => void;
	onNext: () => void;
}) {
	if (loading || error || !records || records.totalItems === 0) return null;
	return (
		<nav
			aria-label="Work record pagination"
			className="mt-6 flex items-center justify-between border-t pt-5"
		>
			<Button
				variant="outline"
				onClick={onPrevious}
				disabled={records.page <= 1}
			>
				<ChevronLeftIcon aria-hidden="true" />
				Previous
			</Button>
			<p className="text-sm text-muted-foreground">
				Page {records.page} of {Math.max(records.totalPages, 1)}
			</p>
			<Button
				variant="outline"
				onClick={onNext}
				disabled={records.page >= records.totalPages}
			>
				Next
				<ChevronRightIcon aria-hidden="true" />
			</Button>
		</nav>
	);
}

export function InvoicesPage() {
	const [draftFilters, setDraftFilters] = useState<Filters>(emptyFilters);
	const [appliedFilters, setAppliedFilters] = useState<Filters>(emptyFilters);
	const [clients, setClients] = useState<string[]>([]);
	const [projects, setProjects] = useState<string[]>([]);
	const [records, setRecords] = useState<WorkRecordPageDto>();
	const [page, setPage] = useState(1);
	const [retryCount, setRetryCount] = useState(0);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState(false);
	const [deleteError, setDeleteError] = useState(false);
	const [deletedDescription, setDeletedDescription] = useState<string>();

	useEffect(() => {
		const controller = new AbortController();
		getWorkRecordOptions(controller.signal)
			.then((options) => {
				setClients(options.clients);
				setProjects(options.projects);
			})
			.catch(() => undefined);
		return () => controller.abort();
	}, []);

	useEffect(() => {
		const controller = new AbortController();
		setLoading(true);
		setError(false);
		getWorkRecords(
			{ ...appliedFilters, page, pageSize: PAGE_SIZE },
			controller.signal,
		)
			.then(setRecords)
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
	}, [appliedFilters, page, retryCount]);

	function updateFilter(name: keyof Filters, value: string) {
		setDraftFilters((current) => ({ ...current, [name]: value }));
	}
	function applyFilters(event: FormEvent<HTMLFormElement>) {
		event.preventDefault();
		setPage(1);
		setAppliedFilters({ ...draftFilters });
	}
	function clearFilters() {
		setDraftFilters(emptyFilters);
		setAppliedFilters(emptyFilters);
		setPage(1);
	}
	async function removeRecord(record: WorkRecordDto) {
		setDeleteError(false);
		setDeletedDescription(undefined);
		try {
			await deleteWorkRecord(record.id);
			setDeletedDescription(record.description);
			if (records?.items.length === 1 && page > 1) {
				setPage((current) => current - 1);
			} else {
				setRetryCount((count) => count + 1);
			}
		} catch (requestError) {
			setDeleteError(true);
			throw requestError;
		}
	}

	const hasFilters = Object.values(appliedFilters).some(Boolean);
	const summary =
		records && records.totalItems > 0
			? `${(records.page - 1) * records.pageSize + 1}-${Math.min(records.page * records.pageSize, records.totalItems)} of ${records.totalItems} work records`
			: !(loading || error)
				? 'No work records'
				: '';

	return (
		<section
			aria-label="Invoices page"
			className="min-h-[calc(100svh-4rem)] bg-muted/30 px-4 py-10 sm:px-6 lg:px-8"
		>
			<div className="mx-auto w-full max-w-6xl">
				<div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
					<div>
						<p className="mb-2 text-sm font-medium text-muted-foreground">
							Invoices
						</p>
						<h1 className="text-3xl font-semibold tracking-tight">
							Work records
						</h1>
						<p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
							Review completed work that is ready for your next
							invoice.
						</p>
					</div>
					<div className="flex flex-wrap gap-2">
						<Button
							variant="outline"
							render={<Link to="/invoices/exported" />}
							nativeButton={false}
							size="lg"
						>
							View invoices
						</Button>
						<Button
							render={<Link to="/work/new" />}
							nativeButton={false}
							size="lg"
						>
							<PlusIcon aria-hidden="true" />
							New work record
						</Button>
					</div>
				</div>
				<FilterPanel
					filters={draftFilters}
					clients={clients}
					projects={projects}
					onChange={updateFilter}
					onSubmit={applyFilters}
					onClear={clearFilters}
				/>
				<div
					aria-live="polite"
					className="mb-4 flex min-h-6 items-center justify-between text-sm text-muted-foreground"
				>
					<p>{error ? '' : summary}</p>
					{loading && <p>Loading work records...</p>}
				</div>
				{deleteError && (
					<p
						role="alert"
						className="mb-4 text-sm text-destructive"
					>
						The work record could not be deleted. Please try again.
					</p>
				)}
				{deletedDescription && (
					<p
						role="status"
						className="mb-4 text-sm text-muted-foreground"
					>
						“{deletedDescription}” was deleted.
					</p>
				)}
				<div aria-busy={loading}>
					<Results
						loading={loading}
						error={error}
						records={records}
						hasFilters={hasFilters}
						onRetry={() => setRetryCount((count) => count + 1)}
						onClear={clearFilters}
						onDelete={removeRecord}
					/>
				</div>
				<Pagination
					records={records}
					loading={loading}
					error={error}
					onPrevious={() => setPage((current) => current - 1)}
					onNext={() => setPage((current) => current + 1)}
				/>
			</div>
		</section>
	);
}
