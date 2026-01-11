import { hiddenProperties, toggleHiddenProperty } from "../Contexts/hiddenPropertySignal";
import { IconButton } from "./IconButton";

export function VisibilityToggle(props: { path: string; }) {
	const isVisible = !hiddenProperties.value.includes(props.path);
		
	const onToggle = (event: Event) => {
		toggleHiddenProperty(props.path);
		event.stopPropagation();
	};
	
	return (
		<IconButton
			noBorder
			title={isVisible ? "Hide property" : "Show property"}
			class="hide-icon"
			onClick={onToggle}
			icon={isVisible ? "visibility" : "visibility_off"}
		/>
	);
}
