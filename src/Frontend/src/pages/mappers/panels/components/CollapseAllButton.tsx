import { propertiesOpenSignal, closeAllProperties } from "@/Contexts/openPropertiesSignal";
import { IconButton } from "../../../../components/IconButton";

export function CollapseAllButton() {
	const expanded = Object.keys(propertiesOpenSignal.value).length > 0;

	return (
		<IconButton
			disableBorder
			tabIndex={0}
			title="Collapse all properties"
			icon="unfold_less"
			disabled={!expanded}
			onClick={closeAllProperties} />
	);
}
