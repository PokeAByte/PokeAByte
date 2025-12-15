import { Toasts } from "../../../notifications/ToastStore";

export function clipboardCopy(value: any) {
	value = value?.toString() ?? "";
	if (value !== "") {
		Toasts.push(`Copied '${value}' to the clipboard`, "info", "blue");
		navigator.clipboard.writeText(value?.toString());
	} else {
		Toasts.push(`No value to copy to clipboard.`, "info", "red");
	}
}