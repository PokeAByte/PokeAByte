import classNames from "classnames";
import type { MaterialIcon } from "material-icons";

export type Props = {
	icon: MaterialIcon,
	disabled?: boolean,
	title: string,
	class?: string,
	tabIndex?: number,
	noBorder?: boolean,
	onClick: (e: UIEvent) => void,
}


export function IconButton(props: Props) {
	return (
		<i
			role="button"
			tabIndex={props.tabIndex ?? 0}
			aria-disabled={props.disabled}
			title={props.title}
			class={classNames("material-icons", {"icon-button": !props.noBorder}, props.class)}
			onClick={props.onClick}
			onKeyUp={e => e.key === "Enter" ? props.onClick(e) : false}
		>
			{props.icon}
		</i>
	);
}