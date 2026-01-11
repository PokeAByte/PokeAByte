import { ComponentChildren } from "preact";
import { hiddenOverrideSignal, hiddenProperties } from "@/Contexts/hiddenPropertySignal";

/**
 * Renders the children only when the path is not a hidden property and the global is not set.
 */
export function IfNotHidden(props: { path: string; children: ComponentChildren; }) {
	if (hiddenOverrideSignal.value || !hiddenProperties.value.includes(props.path)) {
		return props.children;
	}
	return null;
}
