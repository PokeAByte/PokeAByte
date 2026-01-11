import { forceVisible, toggleForceVisible } from "@/Contexts/hiddenPropertySignal";
import { IconButton } from "./IconButton";
import { className } from "@/utility/className";


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
