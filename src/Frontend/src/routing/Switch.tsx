import { VNode } from "preact";
import { pathSignal } from "./Route";
import { normalizePath } from "./normalizePath";


export function Switch(props: { map: [string, () => VNode | null][]; }) {
	const currentPath = pathSignal.value;
	const match = props.map.find(x => currentPath.startsWith(normalizePath(x[0])) || x[0] === "*");
	if (match) {
		return match[1]();
	}
	return null;
}
