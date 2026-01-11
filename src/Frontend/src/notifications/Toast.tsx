import { Icon } from "@/components/Icon";
import { ToastNotification } from "./ToastStore";

export function Toast(props: ToastNotification) {
	return (
		<div role="alert" aria-live="polite" class={"toast " + props.type}>
			<div class="toast-content margin-right">
				<Icon name={props.icon}/>
				<span class="max">
					{props.message.split("\n").map((x, i) => [<span key={i}>{x}</span>, <br/>])}
				</span>
			</div>
			<button type="button" onClick={props.close}>
				<Icon name="close"/>
			</button>
		</div>
	)
}