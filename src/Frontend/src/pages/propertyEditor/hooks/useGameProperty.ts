import { useCallback, useRef, useSyncExternalStore } from "react";
import { Store } from "../../../utility/propertyStore";
import deepEqual from "fast-deep-equal";
import { GameProperty } from "pokeaclient";

export function useGameProperty<T>(path: string) {
	const ref = useRef<GameProperty | null>(null);
	// Cache the result of the store snapshot function (getProperty) to avoid unncessary updates:
	const getProperty = useCallback(
		() => {
			const newProperty = Store.getProperty<T>(path);
			if (!deepEqual(newProperty, ref.current)) {
				ref.current = newProperty;
			}
			return ref.current;
		},
		[path]
	);

	// eslint-disable-next-line react-hooks/exhaustive-deps
	const subscribe = useCallback(
		Store.subscribeProperty(path),
		[path]
	);
	return useSyncExternalStore(subscribe, getProperty);
}
