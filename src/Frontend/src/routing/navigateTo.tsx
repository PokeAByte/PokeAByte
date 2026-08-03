import { normalizePath } from "./normalizePath";

export function navigateTo(path: string) {
	window.history.pushState(null, "", normalizePath(path));
}
