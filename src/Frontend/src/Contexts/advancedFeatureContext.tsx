import { useCallback } from "preact/hooks";
import { ComponentChildren, createContext } from "preact";
import { useStorageState } from "../hooks/useStorageState";

export interface AdvancedFeatureContextData {
	/**
	 * Toggle advanded mode.
	 */
	toggle: () => void,
	/**
	 * Whether to show advanced-mode only UI elements.
	 */
	show: boolean
}

/**
 * Context holding the advanced mode state.
 */
export const AdvancedFeatureContext = createContext<AdvancedFeatureContextData>(null!);

/**
 * Default context provider for the {@link AdvancedFeatureContext}.
 */
export function AdvancedFeatureContextProvider(props: { children: ComponentChildren}) {
	const [advancedMode, setAdvancedMode] = useStorageState("_advandedMode", false);
	const toggle = useCallback(() => {
		setAdvancedMode(!advancedMode);
	}, [setAdvancedMode, advancedMode]);

	return (
		<AdvancedFeatureContext.Provider value={{show: advancedMode, toggle}} >
			{props.children}
		</AdvancedFeatureContext.Provider>
	)
}
