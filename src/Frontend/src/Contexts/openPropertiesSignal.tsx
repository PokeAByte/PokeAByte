import { getStorageItem } from "@/hooks/useStorageState";
import { effect, signal } from "@preact/signals";
import { mapperSignal } from "./mapperSignal";

export const propertiesOpenSignal = signal<Record<string, boolean>>({});
export const attributesOpenSignal = signal<Record<string, boolean>>({});

effect(() => {
	if (mapperSignal.value?.id) {
		propertiesOpenSignal.value = getStorageItem(mapperSignal.value.id, {});
		attributesOpenSignal.value = getStorageItem(mapperSignal.value.id + "-attributes", {});
	}
});
export function togglePropertyOpen(path: string) {
	const mapper = mapperSignal.peek();
	if (!mapper) {
		return;
	}

	const properties = structuredClone(propertiesOpenSignal.peek());
	if (properties[path]) {
		delete properties[path];
	} else {
		properties[path] = true;
	}
	
	propertiesOpenSignal.value = properties;
	window.localStorage.setItem(mapper.id, JSON.stringify(properties));
}
export function toggleAttibutesOpen(path: string) {
	const mapper = mapperSignal.peek();
	if (!mapper) {
		return;
	}

	const properties = structuredClone(attributesOpenSignal.peek());
	if (properties[path]) {
		delete properties[path];
	} else {
		properties[path] = true;
	}
	
	attributesOpenSignal.value = properties;
	window.localStorage.setItem(mapper.id+"-attributes", JSON.stringify(properties));
}

export function closeAllProperties() {
	const mapper = mapperSignal.peek();
	if (!mapper) {
		return;
	}
	propertiesOpenSignal.value = {};
	attributesOpenSignal.value = {};
	window.localStorage.setItem(mapper.id, JSON.stringify({}));
	window.localStorage.setItem(mapper.id+"-attribues", JSON.stringify({}));
}