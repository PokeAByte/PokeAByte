import { Toasts } from "../../../notifications/ToastStore";

export function createMapperLoadToast(_: boolean, result: boolean | string | null) {
	if (result === true) {
		Toasts.push("Loaded mapper", "task_alt", "green");
	} else if (result !== null) {
		Toasts.push("Failed to load mapper:\n " + result, "", "red", false);
	} else {
		Toasts.push("Failed to load mapper.\n Check Poke-A-Byte log for more information.", "", "red");
	}
}
