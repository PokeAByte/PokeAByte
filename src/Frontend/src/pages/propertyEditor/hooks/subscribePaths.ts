import { Store } from "@/utility/propertyStore";

export function subscribePath(path: string, action: (path: string) => void) {
	const onUpdate = (updatedPath: string) => {
		if (updatedPath === path) {
			action(path);
		}
	}
	Store.addUpdateListener(onUpdate);
	return () => Store.removeUpdateListener(onUpdate);
}