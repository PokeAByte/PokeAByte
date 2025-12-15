import classNames from "classnames";
import { useUISetting } from "@/Contexts/UISettingsContext";
import { IconButton } from "./IconButton";

/**
 * The toggle button to activate/deactivate advanced mode.
 */
export function AdvancedToggle() {
	const [advancedMode, setAdvancedMode] = useUISetting("advancedMode");
	return (
		<IconButton
			tabIndex={0}
			noBorder
			title="Toggle advanced mode"
			onClick={() => setAdvancedMode(!advancedMode)}
			class={classNames({ "text-green": advancedMode })}
			icon="rocket"
		/>
	);
}
