import { signal } from "@preact/signals";

export const basePathSignal = signal<string>("")
const locationSignal = signal<Location>(window.location);
export const pathSignal = signal(window.location.pathname);

const onLocationChange = () => {
	locationSignal.value = window.location;
	pathSignal.value = window.location.pathname;
}

export function initializeRouting(basePath: string) {
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
