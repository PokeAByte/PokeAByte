import { getStorageItem } from "../hooks/useStorageState";
import { saveSetting, uiSettingsSignal } from "./uiSettingsSignal";
import { computed, signal } from "@preact/signals"
import { mapperSignal } from "./mapperSignal";

const hiddenPropertySignal = signal<Record<string, string[]>>(getStorageItem("_hiddenProperties", {}));
const advancedMode = computed(() => uiSettingsSignal.value.advancedMode);
export const forceVisible = computed(() => uiSettingsSignal.value.forceVisible);

export const hiddenOverrideSignal = computed(() => {
	return !advancedMode.value || forceVisible.value
});

export const hiddenProperties = computed(() => {
	if (mapperSignal.value) {
		return hiddenPropertySignal.value[mapperSignal.value.id] ?? []
	}
	return [];
})

export const toggleForceVisible = () => {
	saveSetting("forceVisible", !forceVisible.peek())
};

export function toggleHiddenProperty(path: string) {
	const mapper = mapperSignal.peek();
	if (!mapper) {
		return;
	}

	let properties = hiddenProperties.peek();
	if (properties.includes(path)) {
		properties = properties.filter(x => x !== path);
	} else {
		properties = [...properties, path];
	}
	const newValue = {
		... hiddenPropertySignal.peek(),
		[mapper.id]: properties
	}
	hiddenPropertySignal.value = newValue;
	window.localStorage.setItem("_hiddenProperties", JSON.stringify(newValue));
}

