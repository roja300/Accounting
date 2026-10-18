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
					<TooltipTrigger>
						<Button>
							<Link to={'/work/new'}>
								<PlusIcon />
							</Link>
						</Button>
					</TooltipTrigger>
					<TooltipContent>Create new record of work</TooltipContent>
				</Tooltip>
			}
		>
			Invoice Table
		</PageContainer>
	);
}
