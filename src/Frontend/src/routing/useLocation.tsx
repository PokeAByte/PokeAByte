import { useCallback } from "preact/hooks";
import { pathSignal } from "./Route";
import { navigateTo } from "./navigateTo";


export function useLocation(): [string, (path: string, options?: Record<string, any>) => void] {
	const setLocation = useCallback((path: string) => {
		navigateTo(path);
	}, []);
	return [pathSignal.value, setLocation];
}
