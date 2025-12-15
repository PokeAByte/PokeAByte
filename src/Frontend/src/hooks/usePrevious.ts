import { useRef, useEffect } from "preact/hooks";


export function usePrevious<T>(value: T): T {
	const ref = useRef(value);
	useEffect(() => {
		ref.current = value;
	}, [value]);
	// eslint-disable-next-line react-hooks/refs
	return ref.current;
}
