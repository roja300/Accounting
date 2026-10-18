import {
	BanknoteIcon,
	BriefcaseBusinessIcon,
	Building2Icon,
	CalendarDaysIcon,
	CheckCircle2Icon,
} from 'lucide-react';

import {
	Card,
	CardAction,
	CardContent,
	CardDescription,
	CardFooter,
	CardHeader,
	CardTitle,
} from '@/components/ui/card';

const workDetails = [
	{
		label: 'Client',
		description: 'Choose who this work is for',
		icon: Building2Icon,
	},
	{
		label: 'Work date',
		description: 'Set when the work was completed',
		icon: CalendarDaysIcon,
	},
	{
		label: 'Amount',
		description: 'Add the billable value',
		icon: BanknoteIcon,
	},
] as const;

export function NewWorkPage() {
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

				<Card
					role="region"
					aria-labelledby="work-details-title"
					className="relative shadow-sm before:absolute before:inset-y-0 before:left-0 before:w-1 before:bg-primary"
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
									Everything you need to prepare a clear,
									accurate invoice.
								</CardDescription>
							</div>
						</div>
						<CardAction>
							<span className="inline-flex items-center rounded-full border bg-background px-2.5 py-1 text-xs font-medium text-muted-foreground">
								Draft
							</span>
						</CardAction>
					</CardHeader>

					<CardContent className="pl-6 sm:pl-7">
						<div className="grid gap-3 sm:grid-cols-3">
							{workDetails.map((detail) => {
								const Icon = detail.icon;

								return (
									<div
										key={detail.label}
										className="rounded-xl border bg-muted/35 p-4 transition-colors hover:bg-muted/60"
									>
										<div className="mb-3 flex size-8 items-center justify-center rounded-md border bg-background text-foreground shadow-xs">
											<Icon
												className="size-4"
												aria-hidden="true"
											/>
										</div>
										<p className="font-medium">
											{detail.label}
										</p>
										<p className="mt-1 text-xs leading-5 text-muted-foreground">
											{detail.description}
										</p>
									</div>
								);
							})}
						</div>
					</CardContent>

					<CardFooter className="gap-2 pl-6 text-xs text-muted-foreground sm:pl-7">
						<CheckCircle2Icon
							className="size-4 text-foreground"
							aria-hidden="true"
						/>
						Your work stays as a draft until it is ready to invoice.
					</CardFooter>
				</Card>
			</div>
		</section>
	);
}
