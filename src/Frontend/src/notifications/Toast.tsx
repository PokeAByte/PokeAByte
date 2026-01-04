import { ToastNotification } from "./ToastStore";
import classNames from "classnames";

export function Toast(props: ToastNotification) {
	const classes = classNames("toast", props.type);
	return (
		<div role="alert" aria-live="polite" class={classes}>
			<div class="toast-content margin-right">
				<div class="material-icons"> {props.icon} </div>
				<span class="max">
					{props.message.split("\n").map((x, i) => [<span key={i}>{x}</span>, <br/>])}
				</span>
			</div>
			<button type="button" onClick={props.close}>
				<span class="material-icons"> close </span>
			</button>
		</div>
	)
}