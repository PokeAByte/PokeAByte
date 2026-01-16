import { forceVisible, toggleForceVisible } from "@/Contexts/hiddenPropertySignal";
import { IconButton } from "./IconButton";
import { className } from "@/utility/className";
import { closeAllProperties, propertiesOpenSignal } from "@/Contexts/openPropertiesSignal";


export function ForceVisibilityToggle() {
	const override = forceVisible.value;
	return (
		<IconButton
			noBorder
			tabIndex={0}
			title="Toggle forced visibility"
			class={className(override, "text-blue")}
			icon={override ? "visibility" : "visibility_off"}
			onClick={toggleForceVisible}
		/>
	);
}

export function CollapseAllButton() {
	const expanded = Object.keys(propertiesOpenSignal.value).length > 0;

	return (
		<IconButton
			noBorder
			tabIndex={0}
			title="Collapse all properties"
			icon="unfold_less"
			disabled={!expanded}
			onClick={closeAllProperties}
		/>
	);
}
