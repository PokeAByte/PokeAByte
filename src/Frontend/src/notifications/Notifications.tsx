import { useSyncExternalStore } from "preact/compat";
import { useCallback, useRef } from "preact/hooks";
import { ToastNotification, Toasts } from "./ToastStore";
import { Toast } from "./Toast";

export function Notifications() {
	const ref = useRef<ToastNotification[]>([]);
	const getToasts = () => {
		const newToasts = Toasts.getToasts();
		if (newToasts.map(x => x.id.toString()).join() !== ref.current.map(x => x.id.toString()).join()) {
			ref.current = newToasts;
		}
		return ref.current;
	};
	const toasts = useSyncExternalStore(Toasts.subscribe, getToasts);
	const closeAll = useCallback(() => {
		toasts.forEach(toast => Toasts.remove(toast.id));
	}, [toasts])
	return (
		<div class="toasts">
			{toasts.map(toast => {
				return <Toast key={toast.id} {...toast} />
			})}
			{toasts.length > 5 &&
				<button type="button"  class="purple" onClick={closeAll}> 
					Close all <i class="material-icons"> close </i>
				</button>
			}
		</div>
	);
}
