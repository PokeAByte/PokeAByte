import { useSyncExternalStore } from "preact/compat";
import { useCallback, useRef } from "preact/hooks";
import { Store } from "../../../utility/propertyStore";
import { GameProperty } from "pokeaclient";

/**
 * Use a single field of a property from the store.
 *
 * @export
 * @param {string} path The path of the property to select.
 * @param {K} field The property field to select. See {@link GameProperty} definition.
 * @returns {(GameProperty[K]|null)} The property field value, or null if the property does not exist.
 */
export function useGamePropertyField<K extends keyof GameProperty>(path: string, field: K): GameProperty[K] | null {
	const ref = useRef<GameProperty[K] | null>(null);

	// Cache the result of the store snapshot function (getProperty) to avoid unncessary updates:
	const getProperty = useCallback(() => {
		const newProperty = Store.getProperty(path);
		if (newProperty && ref.current && newProperty[field] !== ref.current[field]) {
			ref.current = newProperty[field];
		} else if (!!newProperty !== !!ref.current) {
			ref.current = newProperty ? newProperty[field] : null;
		}
		return ref.current;
	}, [path, field])


	const subscribe = useCallback(
		Store.subscribeProperty(path),
		[path]
	);

	return useSyncExternalStore(subscribe, getProperty);
}