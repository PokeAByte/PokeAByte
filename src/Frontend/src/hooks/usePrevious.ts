import { useRef, useEffect } from "preact/hooks";


export function usePrevious<T>(value: T): T {
	var ref = useRef(value);
	useEffect(() => {
		ref.current = value;
	}, [value]);
	return ref.current;
}
