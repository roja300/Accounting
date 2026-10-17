import { PageContainer } from '@/components/page-container.tsx';
import { Button } from '@/components/ui/button.tsx';
import {
	Table,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from '@/components/ui/table.tsx';

export function PaymentsPage() {
	return (
		<PageContainer
			title={'Payments'}
			headerContent={<Button>Import Payment Data</Button>}
		>
			<Table>
				<TableHeader>
					<TableRow>
						<TableHead>Description</TableHead>
						<TableHead>From</TableHead>
						<TableHead>Amount</TableHead>
						<TableHead>Balance</TableHead>
						<TableHead>Date</TableHead>
						<TableHead>Time</TableHead>
					</TableRow>
				</TableHeader>
				<TableRow>
					<TableCell
						colSpan={10000}
						className="h-24 text-center text-muted-foreground"
					>
						No data found.
					</TableCell>
				</TableRow>
			</Table>
		</PageContainer>
	);
}
