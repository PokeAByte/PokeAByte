import { MapperFile } from "@/api/types";
import { getStorageItem } from "@/hooks/useStorageState";
import { computed, signal } from "@preact/signals";

/** The settings for the Poke-A-Byte frontend. */
export type UISettings = {
	/** Wether the advanced mode is enabled. */
	advancedMode?: boolean,
	/** Whether all properties should be shown, regardless of their hidden status. */
	forceVisible?: boolean,
	/** Which panels the user opened / closed. */
	openPanels: Record<string, boolean | undefined>,
	/** Wether to preserve freezes accross mapper reloads. */
	preserveFreeze?: boolean,
	/** Whether the "recently used mappers" panel is enabled. Default: true. */
	recentlyUsedEnabled?: boolean,
	/** Whether the header should be sticky. Default: true. */
	stickyHeader?: boolean,
	/** Whether the header should be sticky. */
	favoriteMappers?: string[],
	/** The mappers recently loaded by the user. */
	recentMappers?: string[],
}

export const uiSettingsSignal = signal<UISettings>(
	{
		stickyHeader: true,
		recentlyUsedEnabled: true,
		openPanels: {}, 
		...getStorageItem("_uiSettings", {})
	}
);

/** Signal for the advancedMode setting. See {@link UISettings.advancedMode} */
export const advancedModeSignal = computed(() => uiSettingsSignal.value.advancedMode ?? false);

/**
 * Make and save changes to the UI settings.
 */
export function saveSetting<K extends keyof UISettings>(setting: K, value: UISettings[K]) {
	const settings = {
		...uiSettingsSignal.peek(),
		[setting]: value
	};
	uiSettingsSignal.value = settings;
	window.localStorage.setItem("_uiSettings", JSON.stringify(settings));
}

export function getFavoriteId(mapper: MapperFile) {
	return "official_"
		+ mapper.path.split("/").at(1)?.toLowerCase()
		+ "_"
		+ mapper.display_name.replace(".xml", "")
}