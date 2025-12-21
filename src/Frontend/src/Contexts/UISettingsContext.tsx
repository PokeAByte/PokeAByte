import { useContext, useEffect } from "preact/hooks";
import { ComponentChildren, createContext } from "preact";
import { useStorageState } from "@/hooks/useStorageState";

export type UISettings = {
	initialized: boolean,
	advancedMode?: boolean,
	forceVisible?: boolean,
	openPanels: Record<string, boolean | undefined>,
	preserveFreeze?: boolean,
	test?: Record<string, any>,
	recentlyUsedEnabled?: boolean,
	favoriteMappers?: string[],
	recentMappers?: string[],
}

export type  UISettingsContextType = {
	settings: UISettings, 
	save: (v: Partial<UISettings>) => void
}
/**
 * Context holding the UI settings.
 */
export const UISettingsContext = createContext<UISettingsContextType>(null!);


function tryGetLocalStorage<T>(key: string, defaultValue: T) {
	const json = window.localStorage.getItem(key);
	if (!json) {
		return defaultValue;
	}
	try {
		return JSON.parse(json) ?? defaultValue;
	} catch {
		return defaultValue;
	}
}

export function UISettingsProvider(props: { children: ComponentChildren}) {
	const [settings, saveSettings] = useStorageState<UISettings>("_uiSettings", { initialized: false, openPanels: {} });

	const save = (newSettings: Partial<UISettings>) => {
		saveSettings({...settings, ...newSettings});
	};

	useEffect(() => {
		// remove some time after 1.0:
		if (!settings.initialized) {
			saveSettings({
				initialized: true,
				advancedMode: tryGetLocalStorage("_advandedMode", false),
				forceVisible: tryGetLocalStorage("_forceVisible", false),
				openPanels: {},
			});
		}
	}, [settings, saveSettings])

	return (
		<UISettingsContext.Provider value={{settings, save}} >
			{props.children}
		</UISettingsContext.Provider>
	)
}

export function useUISetting<T extends keyof UISettings>(key: T): [UISettings[T], (value: UISettings[T]) => void] {
	const context = useContext(UISettingsContext);
	const set = (value: UISettings[T]) => {
		context.save({[key]: value});
	};
	return [context.settings[key], set];
}