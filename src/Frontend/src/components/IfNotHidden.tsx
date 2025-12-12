import { ComponentChildren } from "preact";
import { useContext } from "preact/hooks";
import { HidePropertyContext } from "../Contexts/HidePropertyContext";

/**
 * Renders the children only when the path is not a hidden property and the global is not set.
 */

export function IfNotHidden(props: { path: string; children: ComponentChildren; }) {
	const context = useContext(HidePropertyContext);
	if (context.override || !context.hiddenProperties.includes(props.path)) {
		return props.children;
	}
	return null;
}
