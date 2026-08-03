import { saveSetting, uiSettingsSignal } from "@/Contexts/uiSettingsSignal";
import { IconButton } from "../components/IconButton";
import { useComputed } from "@preact/signals";
import { className } from "@/utility/className";

/**
 * The toggle button to activate/deactivate advanced mode.
 */
export function AdvancedToggle() {
	const advancedMode = useComputed(() => uiSettingsSignal.value.advancedMode ?? false).value;
	return (
		<IconButton
			tabIndex={0}
			disableBorder
			title="Toggle advanced mode"
			onClick={() => saveSetting("advancedMode", !advancedMode)}
			class={className(advancedMode, "text-green")}
			icon="rocket"
		/>
	);
}
