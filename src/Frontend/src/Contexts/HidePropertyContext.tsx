import { useContext } from "preact/hooks";
import { ComponentChildren, createContext } from "preact";
import { useStorageRecordState } from "../hooks/useStorageState";
import { UISettingsContext } from "./UISettingsContext";

export interface HiddenPropertyData {
	setHidden: (path: string, hide: boolean) => void,
	toggleOverride: () => void,
	hiddenProperties: string[],
	override: boolean,
}

/**
 * Context holding the advanced mode state.
 */
export const HidePropertyContext = createContext<HiddenPropertyData>(null!);

/**
 * Default context provider for the {@link HidePropertyContext}.
 */
export function HidePropertyContextProvider(props: { mapperId: string, children: ComponentChildren}) {
	const settingsContext = useContext(UISettingsContext);
	const toggleOverride = () => settingsContext.save({forceVisible: !settingsContext.settings.forceVisible});
	const [data, setData] = useStorageRecordState<string, string[]>("_hiddenProperties", props.mapperId, []);
	const setHidden = (path: string, hide: boolean) => {
		if (hide && !data.includes(path)) {
			setData([...data, path]);
		}
		if (!hide && data.includes(path)) {
			setData(data.filter(x => x !== path));
		}
	};

	return (
		<HidePropertyContext.Provider value={{
			hiddenProperties: data,
			setHidden,
			toggleOverride: toggleOverride,
			override: settingsContext.settings.forceVisible || !settingsContext.settings.advancedMode
		}} >
			{props.children}
		</HidePropertyContext.Provider>
	)
}
