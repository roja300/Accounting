import { PlusIcon } from 'lucide-react';
import { Link } from 'react-router-dom';
import { PageContainer } from '@/components/page-container.tsx';
import { Button } from '@/components/ui/button.tsx';
import {
	Tooltip,
	TooltipContent,
	TooltipTrigger,
} from '@/components/ui/tooltip.tsx';

export function InvoicesPage() {
	return (
		<PageContainer
			title={'Invoices'}
			headerContent={
				<Tooltip>
					<TooltipTrigger
						render={
							<Button
								render={<Link to={'/work/new'} />}
								size="icon"
								aria-label="Create new work record"
							/>
						}
					>
						<PlusIcon aria-hidden="true" />
					</TooltipTrigger>
					<TooltipContent>Create new record of work</TooltipContent>
				</Tooltip>
			}
		>
			<section aria-label="Invoices page">Invoice Table</section>
		</PageContainer>
	);
}
