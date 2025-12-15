import classNames from "classnames";
import { useContext } from "preact/hooks";
import { HidePropertyContext } from "@/Contexts/HidePropertyContext";
import { IconButton } from "./IconButton";


export function ForceVisibilityToggle() {
	const context = useContext(HidePropertyContext);
	return (
		<IconButton
			noBorder
			tabIndex={0}
			title="Toggle forced visibility"
			class={classNames({ "text-blue": context.override })}
			onClick={() => context.toggleOverride()}
			icon={context.override ? "visibility" : "visibility_off"}
		/>
	);
}
