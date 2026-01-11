import { useCallback,  useEffect,  useState } from "preact/hooks";
import { ToastNotification, Toasts } from "./ToastStore";
import { Toast } from "./Toast";
import { Icon } from "@/components/Icon";

export function Notifications() {
	const [toasts, setToasts] = useState<ToastNotification[]>(Toasts.getToasts())
	const getToasts = () => {
		setToasts(Toasts.getToasts());
	};
	useEffect(() => {
		Toasts.subscribe(getToasts);
	}, []);
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
					Close all <Icon name="close"/>
				</button>
			}
		</div>
	);
}
