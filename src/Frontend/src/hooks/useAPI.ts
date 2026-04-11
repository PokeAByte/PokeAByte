import { useState } from "preact/hooks";

export function useAPI<T extends (...args: any[]) => Promise<any>>(
	api: T,
	chain?: (success: boolean, result: Awaited<ReturnType<T>> | null) => void,
) {
	const [isLoading, setLoading] = useState(false);
	const [wasCalled, setCalled] = useState(false);
	const [result, setResult] = useState<Awaited<ReturnType<T>> | null>(null)
	const call = async (...args: Parameters<T>) => {
		setLoading(true);
		setCalled(true);
		let result = null;
		let success = false;
		try {
			result = await api(...args);
			success = true;
		} catch (exception) {
			result = null;
			console.warn(exception);
		} finally {
			setResult(result);
			setLoading(false);
			if (chain) {
				chain(success, result);
			}
		}
	}
	const reset = () => {
		setCalled(false);
	};

	return {
		isLoading, result, call, wasCalled, reset
	}
}