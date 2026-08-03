import { hiddenProperties, toggleHiddenProperty } from "../../../Contexts/hiddenPropertySignal";
import { IconButton } from "../../../components/IconButton";

/**
 * Hides or unhides the property with the provided path.
 */
export function VisibilityToggle(props: { path: string; }) {
	const isVisible = !hiddenProperties.value.includes(props.path);
	const onToggle = (event: Event) => {
		toggleHiddenProperty(props.path);
		event.stopPropagation();
	};
	
	return (
		<IconButton
			disableBorder
			title={isVisible ? "Hide property" : "Show property"}
			class="hide-icon"
			onClick={onToggle}
			icon={isVisible ? "visibility" : "visibility_off"}
		/>
	);
}
