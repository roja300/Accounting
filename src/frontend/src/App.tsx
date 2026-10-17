import { Navigate, Route, Routes } from 'react-router-dom';

import { AppNavbar } from '@/components/app-navbar/app-navbar';
import { InvoicesPage } from '@/features/invoices/invoices-page';
import {TaxPage} from "@/features/tax/tax-page.tsx";

export function App() {
	return (
		<div className="flex min-h-svh flex-col bg-background">
			<AppNavbar />
			<main className="flex-1">
				<Routes>
					<Route
						path="/"
						element={
							<Navigate
								to="/tax"
								replace
							/>
						}
					/>
					<Route
						path="/"
						element={<TaxPage />}
					/>
					<Route
						path="/tax"
						element={<TaxPage />}
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
