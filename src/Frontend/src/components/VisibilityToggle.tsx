import classNames from "classnames";
import { useContext, useCallback } from "preact/hooks";
import { HidePropertyContext } from "../Contexts/HidePropertyContext";


export function VisibilityToggle(props: { path: string; }) {
	const context = useContext(HidePropertyContext);
	const isVisible = !context.hiddenProperties.includes(props.path);
	const onHideClick = useCallback((event: Event) => {
		context.hideProperty(props.path);
		event.stopPropagation();
	}, [context, isVisible]);
	const onShowClick = useCallback((event: Event) => {
		context.showProperty(props.path);
		event.stopPropagation();
	}, [context, isVisible]);
	if (isVisible) {
		return (
			<i
				role={"button"}
				tabIndex={0}
				title={"Hide property"}
				class={classNames("material-icons icon-button-bare hide-icon")}
				onClick={onHideClick}
			>
				visibility
			</i>
		);
	}
	return (
		<i
			role={"button"}
			tabIndex={0}
			title={"Unhide property"}
			class={classNames("material-icons icon-button-bare hide-icon")}
			onClick={onShowClick}
		>
			visibility_off
		</i>
	);
}
