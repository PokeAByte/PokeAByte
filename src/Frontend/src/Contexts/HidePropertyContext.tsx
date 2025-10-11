import { useCallback, useContext } from "preact/hooks";
import { ComponentChildren, createContext } from "preact";
import { useStorageRecordState, useStorageState } from "../hooks/useStorageState";
import { AdvancedFeatureContext } from "./advancedFeatureContext";

export interface HiddenPropertyData {
	hideProperty: (path: string) => void,
	showProperty: (path: string) => void,
	toggleOverride: () => void,
	hiddenProperties: string[],
	override: boolean,
}

/**
 * Context holding the advanced mode state.
 */
export const HidePropertyContext = createContext<HiddenPropertyData>(null!);

/**
 * Default context provider for the {@link AdvancedFeatureContext}.
 */
export function HidePropertyContextProvider(props: { mapperId: string, children: ComponentChildren}) {
	const advancedFeatureContext = useContext(AdvancedFeatureContext);
	const [data, setData] = useStorageRecordState<string, string[]>("_hiddenProperties", props.mapperId, []);
	const [forceVisible, setForceVisible] = useStorageState<boolean>("_forceVisible", false);
	const hideProperty = useCallback((path: string) => {
		if (!data.includes(path)) {
			setData([...data, path]);
		}
	}, [data, setData]);
	const showProperty = useCallback((path: string) => {
		if (data.includes(path)) {
			setData(data.filter(x => x !== path));
		}
	}, [data, setData]);
	return (
		<HidePropertyContext.Provider value={{
			hiddenProperties: data,
			hideProperty,
			showProperty,
			toggleOverride: () => setForceVisible(!forceVisible),
			override: forceVisible || !advancedFeatureContext.show
		}} >
			{props.children}
		</HidePropertyContext.Provider>
	)
}
