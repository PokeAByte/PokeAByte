import React from "react";


export function useAPI<T extends (...args: any[]) => Promise<any>>(
	api: T,
	chain?: () => void,
) {
	const [isLoading, setLoading] = React.useState(false);
	const [wasCalled, setCalled] = React.useState(false);
	const [result, setResult] = React.useState<Awaited<ReturnType<T>> | null>(null)
	const call = (...args: Parameters<T>) => {
		setLoading(true);
		setCalled(true)
		api(...args)
			.then((result) => {
				setLoading(false);
				setResult(result);
				if (chain) {
					chain();
				}
			})
			.catch(() => {
				setLoading(false)
				setResult(null);
			});
	}
	const reset = () => {
		setCalled(false);
	};

	return {
		isLoading, result, call, wasCalled, reset
	}
}