import type { ReactNode } from 'react';

interface PageContainerProps {
	title: string;
	headerContent?: ReactNode;
	children: ReactNode;
}

export function PageContainer({
	children,
	title,
	headerContent,
}: PageContainerProps) {
	return (
		<div className="flex-1 p-6">
			<div className="flex justify-between">
				<div>
					<p className={'text-3xl font-bold'}>{title}</p>
				</div>
				<div>{headerContent}</div>
			</div>
			<div className={'pt-5'}>{children}</div>
		</div>
	);
}
