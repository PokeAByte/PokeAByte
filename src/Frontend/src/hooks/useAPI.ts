import { useState } from "preact/hooks";

export function useAPI<T extends (...args: any[]) => Promise<any>>(
	api: T,
	chain?: (success: boolean, result: Awaited<ReturnType<T>> | null) => void,
) {
	const [isLoading, setLoading] = useState(false);
	const [wasCalled, setCalled] = useState(false);
	const [result, setResult] = useState<Awaited<ReturnType<T>> | null>(null)
	const call = (...args: Parameters<T>) => {
		setLoading(true);
		setCalled(true)
		api(...args)
			.then((result) => {
				setLoading(false);
				setResult(result);
				if (chain) {
					chain(true, result);
				}
			})
			.catch(() => {
				setLoading(false)
				setResult(null);
				if (chain) {
					chain(false, null);
				}
			});
	}
	const reset = () => {
		setCalled(false);
	};

	return {
		isLoading, result, call, wasCalled, reset
	}
}