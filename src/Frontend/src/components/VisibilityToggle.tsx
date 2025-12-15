import { useContext } from "preact/hooks";
import { HidePropertyContext } from "../Contexts/HidePropertyContext";
import { IconButton } from "./IconButton";


export function VisibilityToggle(props: { path: string; }) {
	const context = useContext(HidePropertyContext);
	const isVisible = !context.hiddenProperties.includes(props.path);
		
	const onToggle = (event: Event) => {
		context.setHidden(props.path, isVisible);
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
