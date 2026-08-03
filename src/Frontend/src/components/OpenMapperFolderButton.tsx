import { openMapperFolder } from "@/api/fetch";
import { Toasts } from "../notifications/ToastStore";
import { WideButton } from "./WideButton";

/** A button to open the mapper folder via the REST api. Also issues a toast notification. */
export function OpenMapperFolderButton() {
	const onClick = () => {
		openMapperFolder().then(
			() => Toasts.push(`Folder opened. Check your file browser.`, "task_alt", "green")
		)
	}
	return <WideButton text="Open mapper folder" color="purple" onClick={onClick} />;
}
