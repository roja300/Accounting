import { Navigate, Route, Routes } from 'react-router-dom';

import { AppNavbar } from '@/components/app-navbar';
import { ExportedInvoicesPage } from '@/features/invoices/exported-invoices-page';
import { InvoicesPage } from '@/features/invoices/invoices-page';
import { NewWorkPage } from '@/features/invoices/new-work-page.tsx';
import { PaymentsPage } from '@/features/payments/payments-page.tsx';
import { ProfilePage } from '@/features/profile/profile-page';
import { TaxPage } from '@/features/tax/tax-page.tsx';

export function App() {
	return (
		<div className="flex min-h-svh flex-col bg-background">
			<AppNavbar />
			<main className="flex-1">
				<Routes>
					<Route
						path="/profile"
						element={<ProfilePage />}
					/>
					<Route
						path="/"
						element={
							<Navigate
								to="/payments"
								replace
							/>
						}
					/>
					<Route
						path="/payments"
						element={<PaymentsPage />}
					/>
					<Route
						path="/tax"
						element={<TaxPage />}
					/>
					<Route
						path="/work/new"
						element={<NewWorkPage />}
					/>
					<Route
						path="/work/:recordId/edit"
						element={<NewWorkPage mode="edit" />}
					/>
					<Route
						path="/invoices/exported"
						element={<ExportedInvoicesPage />}
					/>
					<Route
						path="/invoices"
						element={<InvoicesPage />}
					/>
					<Route
						path="*"
						element={
							<Navigate
								to="/tax"
								replace
							/>
						}
					/>
				</Routes>
			</main>
		</div>
	);
}

export default App;
