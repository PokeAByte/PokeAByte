export interface ToastNotification {
	icon: string,
	type: NotificationColor,
	message: string,
	autoclear: boolean,
	id: number
	clearAt: number,
	close: () => void,
}

export type NotificationColor = "green" | "red" | "blue";

export class ToastStore {
	private _callbacks: (() => void)[] = [];
	private _toasts: ToastNotification[] = [];
	private _lastId: number = 1;

	constructor() {
		window.setInterval(() => this._clear(), 100);
	}

	private _notifySubscribers = () => {
		this._callbacks.forEach(x => x());
	}

	private _clear = () => {
		const now = new Date().getTime();
		const count = this._toasts.length;
		this._toasts = this._toasts.filter(
			x => x.autoclear === false || x.clearAt >= now
		);
		if (this._toasts.length !== count) {
			this._notifySubscribers();
		}
	}

	remove = (id: number) => {
		this._toasts = this._toasts.filter(x => x.id !== id);
		this._notifySubscribers();
	}

	push = (message: string, icon: string = "", type: NotificationColor, autoclear: boolean = true) => {
		const id = ++this._lastId;
		const existingNotification = this._toasts.find(
				x => x.message.startsWith(message) 
					&& x.icon === icon 
					&& x.type === type
					&& x.autoclear === false
		);
		if (existingNotification) {
			const counter = existingNotification.message.match(/\(x\d+\)$/);
			let count = 1;
			if (counter) {
				count = parseInt(counter[0].substring(2, counter[0].length-1));
				count++;
			}
			existingNotification.message = message + ` (x${count})`;
		} else {
			this._toasts.push({
				message,
				icon,
				type,
				autoclear,
				id,
				close: () => this.remove(id),
				clearAt: autoclear ? new Date().getTime() + 5000 : 0,
			});
		}
		this._notifySubscribers();
	}

	subscribe = (onStoreChange: () => void) => {
		this._callbacks.push(onStoreChange);
		return () => {
			this._callbacks = this._callbacks.filter(x => x !== onStoreChange);
		}
	}
	getToasts = () => [...this._toasts];
}

export const Toasts = new ToastStore();