import { ComponentChildren } from "preact";
import { useContext } from "preact/hooks";
import { AdvancedFeatureContext } from "../Contexts/advancedFeatureContext";

/**
 * Renders the children only when advanced mode is disabled.
 */
export function NotAdvanced(props: { children: ComponentChildren; }) {
	const context = useContext(AdvancedFeatureContext);
	if (!context.show) {
		return props.children;
	}
	return null;
}
