import { Toasts } from "../notifications/ToastStore";
import { Store } from "../utility/propertyStore";
import { WideButton } from "./WideButton";

export function OpenMapperFolderButton() {
	const onClick = () => {
		Store.client.files.openMapperFolder().then(
			() => Toasts.push(`Folder opened. Check your file browser.`, "task_alt", "green")
		)
	}
	return <WideButton text="Open mapper folder" color="purple" onClick={onClick} />;
}
