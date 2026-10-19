import { DownloadIcon, FileTextIcon, LoaderCircleIcon } from 'lucide-react';
import { useEffect, useState } from 'react';

import { Button } from '@/components/ui/button';
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
import { Textarea } from '@/components/ui/textarea';
import {
	type GeneratedInvoiceDto,
	type GenerateInvoiceOptionsDto,
	generatedInvoicePdfUrl,
	generateInvoice,
	getGenerateInvoiceOptions,
} from '@/features/invoices/exported-invoice-api';

interface ExportInvoiceDialogProps {
	open: boolean;
	onOpenChange: (open: boolean) => void;
	onGenerated: () => void;
}

function isoDate(date: Date) {
	return date.toISOString().slice(0, 10);
}

function initialDates() {
	const issueDate = new Date();
	const dueDate = new Date(issueDate);
	dueDate.setDate(dueDate.getDate() + 30);
	return { issueDate: isoDate(issueDate), dueDate: isoDate(dueDate) };
}

export function ExportInvoiceDialog({
	open,
	onOpenChange,
	onGenerated,
}: ExportInvoiceDialogProps) {
	const [options, setOptions] = useState<GenerateInvoiceOptionsDto>();
	const [clientId, setClientId] = useState('');
	const [projectId, setProjectId] = useState('');
	const [currency, setCurrency] = useState<'GBP' | 'EUR' | 'USD'>('GBP');
	const [{ issueDate, dueDate }, setDates] = useState(initialDates);
	const [notes, setNotes] = useState('');
	const [loading, setLoading] = useState(false);
	const [error, setError] = useState('');
	const [generated, setGenerated] = useState<GeneratedInvoiceDto>();

	useEffect(() => {
		if (!open) return;
		const controller = new AbortController();
		setError('');
		getGenerateInvoiceOptions(controller.signal)
			.then(setOptions)
			.catch((requestError: unknown) => {
				if (
					!(
						requestError instanceof DOMException &&
						requestError.name === 'AbortError'
					)
				) {
					setError('Invoice options could not be loaded.');
				}
			});
		return () => controller.abort();
	}, [open]);

	function close(nextOpen: boolean) {
		if (!(nextOpen || loading)) {
			setGenerated(undefined);
			setError('');
		}
		onOpenChange(nextOpen);
	}

	async function submit(event: React.FormEvent<HTMLFormElement>) {
		event.preventDefault();
		setLoading(true);
		setError('');
		try {
			const invoice = await generateInvoice({
				clientId,
				projectId: projectId || undefined,
				currency,
				issueDate,
				dueDate,
				notes: notes.trim() || undefined,
			});
			setGenerated(invoice);
			onGenerated();
		} catch (requestError) {
			setError(
				requestError instanceof Error
					? requestError.message
					: 'The invoice could not be generated.',
			);
		} finally {
			setLoading(false);
		}
	}

	return (
		<Dialog
			open={open}
			onOpenChange={close}
		>
			<DialogContent className="max-h-[calc(100svh-2rem)] overflow-y-auto sm:max-w-xl">
				{generated ? (
					<>
						<DialogHeader>
							<DialogTitle>Invoice ready</DialogTitle>
							<DialogDescription>
								{generated.invoiceNumber} has been saved and its
								work records are now marked as exported.
							</DialogDescription>
						</DialogHeader>
						<div className="rounded-lg border bg-muted/40 p-4">
							<p className="font-medium">
								{generated.clientName}
							</p>
							<p className="mt-1 text-sm text-muted-foreground">
								{generated.lines.length} work{' '}
								{generated.lines.length === 1
									? 'record'
									: 'records'}{' '}
								· {generated.currency}{' '}
								{generated.subtotal.toFixed(2)}
							</p>
						</div>
						<DialogFooter>
							<Button
								variant="outline"
								onClick={() => close(false)}
							>
								Close
							</Button>
							<Button
								render={
									<a
										href={generatedInvoicePdfUrl(
											generated.id,
										)}
										aria-label={`Download ${generated.invoiceNumber} PDF`}
										target="_blank"
										rel="noreferrer"
									>
										<span className="sr-only">
											Download invoice PDF
										</span>
									</a>
								}
								nativeButton={false}
							>
								<DownloadIcon aria-hidden="true" />
								Download PDF
							</Button>
						</DialogFooter>
					</>
				) : (
					<form onSubmit={submit}>
						<DialogHeader>
							<DialogTitle>Generate invoice</DialogTitle>
							<DialogDescription>
								Choose the client and optional project. All
								matching unexported work records in the selected
								currency will be included.
							</DialogDescription>
						</DialogHeader>
						<div className="grid gap-5 py-2 sm:grid-cols-2">
							<div className="space-y-2 sm:col-span-2">
								<Label htmlFor="export-client">Client</Label>
								<p
									id="export-client-description"
									className="text-xs text-muted-foreground"
								>
									Only clients with work ready to invoice are
									shown.
								</p>
								<Select
									value={clientId}
									onValueChange={(value) =>
										setClientId(value ?? '')
									}
									required
								>
									<SelectTrigger
										id="export-client"
										aria-describedby="export-client-description"
										className="w-full"
									>
										<SelectValue placeholder="Select a client" />
									</SelectTrigger>
									<SelectContent
										align="start"
										alignItemWithTrigger={false}
									>
										{options?.clients.map((client) => (
											<SelectItem
												key={client.id}
												value={client.id}
											>
												{client.name}
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>
							<div className="space-y-2 sm:col-span-2">
								<Label htmlFor="export-project">Project</Label>
								<p
									id="export-project-description"
									className="text-xs text-muted-foreground"
								>
									Leave this as all projects to combine the
									client's work.
								</p>
								<Select
									value={projectId || 'all'}
									onValueChange={(value) =>
										setProjectId(
											value === 'all'
												? ''
												: (value ?? ''),
										)
									}
								>
									<SelectTrigger
										id="export-project"
										aria-describedby="export-project-description"
										className="w-full"
									>
										<SelectValue />
									</SelectTrigger>
									<SelectContent
										align="start"
										alignItemWithTrigger={false}
									>
										<SelectItem value="all">
											All projects
										</SelectItem>
										{options?.projects.map((project) => (
											<SelectItem
												key={project.id}
												value={project.id}
											>
												{project.name}
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>
							<div className="space-y-2">
								<Label htmlFor="export-issue-date">
									Issue date
								</Label>
								<Input
									id="export-issue-date"
									type="date"
									value={issueDate}
									max={dueDate}
									onChange={(event) =>
										setDates((dates) => ({
											...dates,
											issueDate: event.target.value,
										}))
									}
									required
								/>
							</div>
							<div className="space-y-2">
								<Label htmlFor="export-due-date">
									Due date
								</Label>
								<Input
									id="export-due-date"
									type="date"
									value={dueDate}
									min={issueDate}
									onChange={(event) =>
										setDates((dates) => ({
											...dates,
											dueDate: event.target.value,
										}))
									}
									required
								/>
							</div>
							<div className="space-y-2 sm:col-span-2">
								<Label htmlFor="export-currency">
									Currency
								</Label>
								<p
									id="export-currency-description"
									className="text-xs text-muted-foreground"
								>
									Work in other currencies will remain ready
									for a separate invoice.
								</p>
								<Select
									value={currency}
									onValueChange={(value) =>
										setCurrency(
											(value ?? 'GBP') as typeof currency,
										)
									}
								>
									<SelectTrigger
										id="export-currency"
										aria-describedby="export-currency-description"
										className="w-full"
									>
										<SelectValue />
									</SelectTrigger>
									<SelectContent
										align="start"
										alignItemWithTrigger={false}
									>
										{(['GBP', 'EUR', 'USD'] as const).map(
											(value) => (
												<SelectItem
													key={value}
													value={value}
												>
													{value}
												</SelectItem>
											),
										)}
									</SelectContent>
								</Select>
							</div>
							<div className="space-y-2 sm:col-span-2">
								<Label htmlFor="export-notes">
									Payment notes
								</Label>
								<p
									id="export-notes-description"
									className="text-xs text-muted-foreground"
								>
									Optional terms or payment instructions shown
									on the PDF.
								</p>
								<Textarea
									id="export-notes"
									aria-describedby="export-notes-description"
									value={notes}
									onChange={(event) =>
										setNotes(event.target.value)
									}
									maxLength={500}
									placeholder="e.g. Payment due within 30 days"
								/>
							</div>
						</div>
						{error && (
							<p
								className="mb-4 text-sm text-destructive"
								role="alert"
							>
								{error}
							</p>
						)}
						{options && options.clients.length === 0 && (
							<p className="mb-4 text-sm text-muted-foreground">
								There are no unexported work records ready to
								invoice.
							</p>
						)}
						<DialogFooter>
							<Button
								type="button"
								variant="outline"
								onClick={() => close(false)}
								disabled={loading}
							>
								Cancel
							</Button>
							<Button
								type="submit"
								disabled={
									loading ||
									!clientId ||
									!options?.clients.length
								}
							>
								{loading ? (
									<LoaderCircleIcon
										className="animate-spin"
										aria-hidden="true"
									/>
								) : (
									<FileTextIcon aria-hidden="true" />
								)}
								{loading ? 'Generating…' : 'Generate invoice'}
							</Button>
						</DialogFooter>
					</form>
				)}
			</DialogContent>
		</Dialog>
	);
}
