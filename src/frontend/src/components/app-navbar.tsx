import { CircleUserRoundIcon, MoonIcon, SunIcon } from 'lucide-react';
import { Link, NavLink } from 'react-router-dom';

import { useTheme } from '@/components/theme-provider';
import { Button } from '@/components/ui/button';
import {
	Tooltip,
	TooltipContent,
	TooltipTrigger,
} from '@/components/ui/tooltip';
import { cn } from '@/lib/utils';

const navigationItems = [
	{ label: 'Payments', to: '/payments' },
	{ label: 'Tax', to: '/tax' },
	{ label: 'Work records', to: '/invoices' },
	{ label: 'Invoices', to: '/invoices/exported' },
] as const;

export function AppNavbar() {
	const { setTheme } = useTheme();

	function toggleTheme() {
		const isDark = document.documentElement.classList.contains('dark');
		setTheme(isDark ? 'light' : 'dark');
	}

	return (
		<header className="sticky top-0 z-50 shrink-0 border-b bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/80">
			<div className="mx-auto flex h-16 w-full max-w-screen-2xl items-center gap-5 px-4 sm:px-6 lg:px-8">
				<Link
					to="/payments"
					className="shrink-0 rounded-sm font-semibold tracking-tight outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
				>
					Accounting
				</Link>
				<div
					className="h-5 w-px bg-border"
					aria-hidden="true"
				/>
				<nav
					aria-label="Primary navigation"
					className="flex items-center gap-1"
				>
					{navigationItems.map((item) => (
						<NavLink
							key={item.to}
							to={item.to}
							end
							className={({ isActive }) =>
								cn(
									'rounded-md px-3 py-2 text-sm font-medium outline-none transition-colors focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
									isActive
										? 'bg-accent text-accent-foreground'
										: 'text-muted-foreground hover:bg-accent/60 hover:text-foreground',
								)
							}
						>
							{item.label}
						</NavLink>
					))}
				</nav>
				<div className="ml-auto flex items-center gap-1">
					<Tooltip>
						<TooltipTrigger
							render={
								<Button
									variant="ghost"
									size="icon-lg"
									className="rounded-full text-muted-foreground"
									aria-label="Toggle colour theme"
									onClick={toggleTheme}
								>
									<MoonIcon
										className="dark:hidden"
										aria-hidden="true"
									/>
									<SunIcon
										className="hidden dark:block"
										aria-hidden="true"
									/>
								</Button>
							}
						/>
						<TooltipContent
							role="tooltip"
							side="bottom"
						>
							Toggle light or dark mode. Press D anywhere.
						</TooltipContent>
					</Tooltip>
					<NavLink
						to="/profile"
						aria-label="Business profile"
						className={({ isActive }) =>
							cn(
								'flex size-9 items-center justify-center rounded-full outline-none transition-colors focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
								isActive
									? 'bg-primary text-primary-foreground'
									: 'text-muted-foreground hover:bg-accent hover:text-foreground',
							)
						}
					>
						<CircleUserRoundIcon
							className="size-6"
							aria-hidden="true"
						/>
					</NavLink>
				</div>
			</div>
		</header>
	);
}
