import { ComponentChildren } from "preact";
import { useContext } from "preact/hooks";
import { AdvancedFeatureContext } from "./advancedFeatureContext";

/**
 * Renders the children only when advanced mode is enabled.
 */
export function Advanced(props: { children: ComponentChildren; }) {
	const context = useContext(AdvancedFeatureContext);
	if (context.show) {
		return props.children;
	}
	return null;
}
