import { useCallback, useContext } from "preact/hooks";
import { ComponentChildren, createContext } from "preact";
import { useStorageRecordState, useStorageState } from "../hooks/useStorageState";
import classNames from "classnames";
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
/**
 * Renders the children only when the path is not a hidden property and the global is not set.
 */
export function IfNotHidden(props: { path: string, children: ComponentChildren; }) {
	const context = useContext(HidePropertyContext);
	if (context.override || !context.hiddenProperties.includes(props.path)) {
		return props.children;
	}
	return null;
}

export function ToggleHidden(props: {path: string}) {
	const context = useContext(HidePropertyContext);
	const isVisible = !context.hiddenProperties.includes(props.path);
	const onHideClick = useCallback((event: Event) => {
		context.hideProperty(props.path)
		event.stopPropagation();
	}, [context, isVisible]);
	const onShowClick = useCallback((event: Event) => {
		context.showProperty(props.path)
		event.stopPropagation();
	}, [context, isVisible]);
	if (isVisible) {
		return (
			<i
				role={"button"}
				tabIndex={0}
				title={"Hide property"}
				class={classNames("material-icons icon-button-bare hide-icon")}
				onClick={onHideClick}
			>
				visibility
			</i>
		);
	} 
	return (
		<i
			role={"button"}
			tabIndex={0}
			title={"Unhide property"}
			class={classNames("material-icons icon-button-bare hide-icon")}
			onClick={onShowClick}
		>
			visibility_off
		</i>
	);
}

export function ToggleForceVisible() {
	const context = useContext(HidePropertyContext);
	return (
		<i
			role={"button"}
			tabIndex={0}
			title={"Toggle forced visibility"}
			class={classNames("material-icons icon-button-bare", { "text-blue": context.override })}
			onClick={() => context.toggleOverride()}
		>
			{context.override ? "visibility" : "visibility_off" }
		</i>
	);
}