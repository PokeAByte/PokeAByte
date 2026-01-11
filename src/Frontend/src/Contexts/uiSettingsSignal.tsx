import { getStorageItem } from "@/hooks/useStorageState";
import { computed, signal } from "@preact/signals";

export type UISettings = {
	initialized: boolean,
	advancedMode?: boolean,
	forceVisible?: boolean,
	openPanels: Record<string, boolean | undefined>,
	preserveFreeze?: boolean,
	recentlyUsedEnabled?: boolean,
	stickyHeader?: boolean,
	favoriteMappers?: string[],
	recentMappers?: string[],
}

export const uiSettingsSignal = signal<UISettings>(
	getStorageItem("_uiSettings", { initialized: false, openPanels: {} })
);

export const advancedModeSignal = computed(() => uiSettingsSignal.value.advancedMode ?? false);

export function saveSetting<K extends keyof UISettings>(setting: K, value: UISettings[K]) {
	const settings = {
		...uiSettingsSignal.peek(),
		[setting]: value
	};
	uiSettingsSignal.value = settings;
	window.localStorage.setItem("_uiSettings", JSON.stringify(settings));
}
