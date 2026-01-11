import { signal } from "@preact/signals";
import { VNode } from "preact";
import { useCallback } from "preact/hooks";

const basePathSignal = signal<string>("")
const locationSignal = signal<Location>(window.location);
const pathSignal = signal(window.location.pathname);

const onLocationChange = () => {
	locationSignal.value = window.location;
	pathSignal.value = window.location.pathname;
}

export function initializeRouting(basePath: string) {
	// const orginalPushState = window.history.pushState;
	window.history.pushState = new Proxy(window.history.pushState, {
		apply(target, scope, argumentsList: Parameters<typeof window.history.pushState> ) {
			const result = target.apply(scope, argumentsList);
			onLocationChange();
			return result;
		},
	});

	window.onpopstate = () => onLocationChange();
	window.onhashchange = () => onLocationChange();

	window.onclick = function(event) {
		if (event.target && event.target instanceof HTMLAnchorElement ) {
			const {href, target} = event.target;
			if (href.startsWith(window.location.origin) && !target) {
				if (href !== window.location.toString()) {
					window.history.pushState(null, "", href);
				}
				event.preventDefault();
			}
		}
	}

	basePathSignal.value = basePath;
	if (!window.location.pathname.startsWith(basePath)) {
		document.location = basePath;
	}
}

const normalizePath = (path: string) => {
	if (!path.startsWith(basePathSignal.peek())) {
		path = basePathSignal.peek() + path;
	}
	return path;
}

export function navigateTo(path: string) {
	window.history.pushState(null, "", normalizePath(path));
}

export function useLocation(): [string, (path: string, options?: Record<string, any>) => void] {
	const setLocation = useCallback((path: string) => {
		navigateTo(path);
	}, []);
	return [pathSignal.value, setLocation];
}

export function Switch(props: {map: [string, () => VNode|null][]}) {
	const currentPath = pathSignal.value;
	const match = props.map.find(x => currentPath.startsWith(normalizePath(x[0])) || x[0] === "*");
	if (match) {
		return match[1]();
	}
	return null;
}