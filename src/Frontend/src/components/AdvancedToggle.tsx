import { saveSetting, uiSettingsSignal } from "@/Contexts/uiSettingsSignal";
import { IconButton } from "./IconButton";
import { useComputed } from "@preact/signals";
import { className } from "@/utility/className";

const setAdvancedMode = (value: boolean) => saveSetting("advancedMode", value);

/**
 * The toggle button to activate/deactivate advanced mode.
 */
export function AdvancedToggle() {
	const advancedMode = useComputed(() => uiSettingsSignal.value.advancedMode ?? false).value;
	return (
		<IconButton
			tabIndex={0}
			noBorder
			title="Toggle advanced mode"
			onClick={() => setAdvancedMode(!advancedMode)}
			class={className(advancedMode, "text-green")}
			icon="rocket"
		/>
	);
}
