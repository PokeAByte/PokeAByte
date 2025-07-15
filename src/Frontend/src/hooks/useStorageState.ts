import { useCallback, useState } from "preact/hooks";

export function useStorageState<T>(key: string, defaultValue: T): [T, (value: T) => void] {
	const [item, setItem] = useState<T>(
		() => {
			const json = window.localStorage.getItem(key);
			if (!json) {
				return defaultValue;
			}
			try {
				return JSON.parse(json) ?? defaultValue;
			} catch (e) {
				console.log(e);
				return defaultValue;
			}
		}
	);
	const setter = useCallback((newValue: T) => {
		setItem(newValue);
		window.localStorage.setItem(key, JSON.stringify(newValue));
	}, [setItem, key]);
	return [item, setter]
}

export function useStorageRecordState<K extends string, V>(
	name: string,
	key: K,
	defaultValue: V
):[V, (value: V) => void] {
	const getStoredRecord = useCallback(() => {
		const json = window.localStorage.getItem(name);
		let record: Record<K, V> = {} as Record<K, V> ;
		if (json) {
			try {
				record = JSON.parse(json) as Record<K, V>  ?? {};
			} catch {
			}
		}
		return record;
	}, [])
	const [item, setItem] = useState<V>(
		() => {
			const record = getStoredRecord();
			return record[key] ?? defaultValue;
		}
	);
	const setter = useCallback((newValue: V) => {
		setItem(newValue);
		let record = getStoredRecord();
		if (newValue === false) {
			delete record[key];
		} else {
			record[key] = newValue;
		}
		window.localStorage.setItem(name, JSON.stringify(record));
	}, [setItem]);
	return [item, setter]
}