import { openMapperFolder } from "@/utility/fetch";
import { Toasts } from "../notifications/ToastStore";
import { WideButton } from "./WideButton";

export function OpenMapperFolderButton() {
	const onClick = () => {
		openMapperFolder().then(
			() => Toasts.push(`Folder opened. Check your file browser.`, "task_alt", "green")
		)
	}
	return <WideButton text="Open mapper folder" color="purple" onClick={onClick} />;
}
