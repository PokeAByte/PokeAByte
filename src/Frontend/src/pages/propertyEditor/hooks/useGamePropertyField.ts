import { useCallback, useEffect, useState } from "preact/hooks";
import { Store } from "../../../utility/propertyStore";
import { GameProperty } from "pokeaclient";
import { subscribePath } from "./subscribePaths";

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
	const onPropertyChange = useCallback(() => {
		const newProperty = Store.getProperty(path);
		setData(newProperty ? newProperty[field] : null);
	}, [path, field]);
	
	useEffect(() => {
		if (path !== "") {
			return subscribePath(path, onPropertyChange);
		}
	}, [onPropertyChange, path]);
	return data;
}