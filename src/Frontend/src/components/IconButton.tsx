import { className } from "@/utility/className";
import type { MaterialIcon } from "material-icons";

export type IconButtonProps = {
	/** Which material icon to use for the button.  */
	icon: MaterialIcon,
	/** Whether the button is disable / uninteractive  */
	disabled?: boolean,
	/** The title / acccesible name of the button. */
	title: string,
	/** Additional css class for the button. */	
	class?: string,
	/** Manual tabIndex for the button. */
	tabIndex?: number,
	/** If true, the 'icon-button' class is not applied, disabling the border styling for the button. */
	disableBorder?: boolean,
	/** Function called when the button is clicked on. */
	onClick: (event: UIEvent) => void,
}

/** Renders an icon with the button role that can be clicked.  */
export function IconButton(props: IconButtonProps) {
	return (
		<i
			role="button"
			tabIndex={props.tabIndex ?? 0}
			aria-disabled={props.disabled}
			title={props.title}
			aria-label={props.title}
			class={"material-icons " + (props.class??"") + " " + className(!props.disableBorder, "icon-button")}
			onClick={props.onClick}
			onKeyUp={e => e.key === "Enter" ? props.onClick(e) : false}
		>
			{props.icon}
		</i>
	);
}