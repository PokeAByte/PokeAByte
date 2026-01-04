import { useEffect, useState } from "preact/hooks";
import { Store } from "../../../utility/propertyStore";
import { GameProperty } from "pokeaclient";

/**
 * Use a single field of a property from the store.
 *
 * @export
 * @param {string} path The path of the property to select.
 * @param {Key} field The property field to select. See {@link GameProperty} definition.
 * @returns {(GameProperty[Key]|null)} The property field value, or null if the property does not exist.
 */
export function useGamePropertyField<K extends keyof GameProperty>(path: string, field: K): GameProperty[K] | null {
	const [data, setData] = useState(() => {
		const prop = Store.getProperty(path)
		return prop ? prop[field] : null;
	})
	useEffect(
		() => {
			const getProperty = (updatedPath: string) => {
				if (updatedPath === path) {
					const newProperty = Store.getProperty(path);
					setData(newProperty ? newProperty[field] : null);
				}
			};
			Store.addUpdateListener(getProperty);
			return () => Store.removeUpdateListener(getProperty);
		},
		[path, field]
	);
	return data;
}