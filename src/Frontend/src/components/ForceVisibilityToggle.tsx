import classNames from "classnames";
import { useContext } from "preact/hooks";
import { HidePropertyContext } from "../Contexts/HidePropertyContext";


export function ForceVisibilityToggle() {
	const context = useContext(HidePropertyContext);
	return (
		<i
			role={"button"}
			tabIndex={0}
			title={"Toggle forced visibility"}
			class={classNames("material-icons icon-button-bare", { "text-blue": context.override })}
			onClick={() => context.toggleOverride()}
		>
			{context.override ? "visibility" : "visibility_off"}
		</i>
	);
}
