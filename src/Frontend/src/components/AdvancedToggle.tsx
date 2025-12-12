import classNames from "classnames";
import { useContext } from "preact/hooks";
import { AdvancedFeatureContext } from "../Contexts/advancedFeatureContext";

/**
 * The toggle button to activate/deactivate advanced mode.
 */
export function AdvancedToggle() {
	const context = useContext(AdvancedFeatureContext);
	return (
		<i
			role={"button"}
			tabIndex={0}
			title={"Toggle advanced mode"}
			class={classNames("material-icons icon-button-bare", { "text-green": context.show })}
			onClick={context.toggle}
		>
			rocket
		</i>
	);
}
