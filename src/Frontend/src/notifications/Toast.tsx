import { ToastNotification } from "./ToastStore";
import classNames from "classnames";

export function Toast(props: ToastNotification) {
	const classes = classNames(
		"toast",
		{
			"blue": props.type === "blue",
			"red": props.type === "error",
			"green": props.type === "success",
		}
	);
	return (
		<div role="alert" aria-live="polite" className={classes}>
			<div className="toast-content margin-right">
				<div className="material-icons"> {props.icon} </div>
				<span className="max">
					{props.message}
				</span>
			</div>
			<button type="button" onClick={props.close}>
				<span className="material-icons"> close </span>
			</button>
		</div>
	)
}