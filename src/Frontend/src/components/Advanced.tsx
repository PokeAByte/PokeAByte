import { ComponentChildren } from "preact";
import { useUISetting } from "@/Contexts/UISettingsContext";

type Props = {
	when?: boolean,
	children: ComponentChildren
};

/**
 * Renders the children depending on wether advanced mode is enabled or not. Defaults to rendering when the 
 * mode is enabled. See {@link Props.when}
 */
export function Advanced({when: is = true, children}: Props) {
	const [advancedMode] = useUISetting("advancedMode");
	if (advancedMode === is) {
		return children;
	}
	return null;
}