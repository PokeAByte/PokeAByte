import { useCallback, useState } from "preact/hooks";


export function getStorageItem<T>(key: string, defaultValue: T) {
	const json = window.localStorage.getItem(key);
	let value = defaultValue;
	try {
		if (json) {
			value = JSON.parse(json) ?? defaultValue;
		}
	} catch (e) {
		console.log(e);
	}
	return value;
}

export function useStorageState<T>(key: string, defaultValue: T): [T, (value: T) => void] {
	const [item, setItem] = useState<T>(
		() => {
			return getStorageItem(key, defaultValue);
		}
	);
	
	const setter = useCallback((newValue: T) => {
		setItem(structuredClone(newValue));		
		window.localStorage.setItem(key, JSON.stringify(newValue));
	}, [setItem, key]);

	return [item, setter]
}

