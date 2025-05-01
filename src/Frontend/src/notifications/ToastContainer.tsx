import { useCallback, useRef, useSyncExternalStore } from "react";
import { ToastNotification, Toasts } from "./ToastStore";

export function ToastContainer() {
	const ref = useRef<ToastNotification[]>([]);
	const getToasts = useCallback(() => {
		const newToasts = Toasts.getToasts();
		if (newToasts.map(x => x.id.toString()).join() !== ref.current.map(x => x.id.toString()).join()) {
			ref.current = newToasts;
		}
		return ref.current;
	}, [])
	const toasts = useSyncExternalStore(Toasts.subscribe, getToasts);
	return (
		<div className="toast-container">
			{toasts.map(toast => {
				return <Toast key={toast.id} {...toast} />
			})}
		</div>
	);
}

export function Toast(props: ToastNotification) {
	const classes = `toast ${props.type}`;
	return (
		<div role="alert" aria-live="polite" className={classes}>
			<div className="toast-content">
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