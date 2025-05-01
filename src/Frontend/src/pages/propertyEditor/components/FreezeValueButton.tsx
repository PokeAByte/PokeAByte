import { Store } from "../../../utility/propertyStore";
import { Toasts } from "../../../notifications/ToastStore";

export function FreezeValueButton({ isFrozen, path }: { isFrozen: boolean, path: string }) {
	const handleClick = () => {
		Store.client.freezeProperty(path, !isFrozen).then(() => Toasts.push(`Saved successful`, "task_alt", "success"));
	}
	const classes = isFrozen ? "margin-left border-blue highlight" : "margin-left";
	return (
		<button className={classes} type="button" onClick={handleClick}>
			<i className="material-icons"> ac_unit </i>
		</button>
	)
}

