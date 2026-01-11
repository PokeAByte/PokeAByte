import { navigateTo } from "@/components/Route";
import { Toasts } from "../../../notifications/ToastStore";

export function onMapperLoaded(_: boolean, result: boolean | string | null) {
	if (result === true) {
		Toasts.push("Loaded mapper", "task_alt", "green");
		navigateTo("/properties");
	} else if (result !== null) {
		Toasts.push("Failed to load mapper:\n " + result, "", "red", false);
	} else {
		Toasts.push("Failed to load mapper.\n Check Poke-A-Byte log for more information.", "", "red");
	}
}
