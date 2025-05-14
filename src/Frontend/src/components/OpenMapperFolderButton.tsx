import { useCallback } from "preact/hooks";
import { Toasts } from "../notifications/ToastStore";
import { Store } from "../utility/propertyStore";

export function OpenMapperFolderButton() {
	const onClick = useCallback(() => {
		Store.client.files.openMapperFolder().then(

			() => Toasts.push(`Folder opened. Check your file browser.`, "task_alt", "success")
		)
		}, []);
	return (
		<button className="purple wide-button" onClick={onClick}>
			Open mapper folder
		</button>
	);
}
