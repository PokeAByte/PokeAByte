import { basePathSignal } from "./Route";


export const normalizePath = (path: string) => {
	if (!path.startsWith(basePathSignal.peek())) {
		path = basePathSignal.peek() + path;
	}
	return path;
};
