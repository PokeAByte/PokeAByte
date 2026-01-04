import { useEffect, useState } from "preact/hooks";
import { Store } from "../../../utility/propertyStore";
import { GameProperty } from "pokeaclient";

export function useGameProperty<T>(path: string) {
	const [property, setProperty] = useState<GameProperty<T>|null>(() => Store.getProperty(path));
	useEffect(
		() => {
			const callback = (updatedPath: string) => {
				if (updatedPath === path) {
					setProperty(Store.getProperty(path))
				}
			};
			Store.addUpdateListener(callback);
			return () => Store.removeUpdateListener(callback);
		},
		[path]
	);
	return property;
}
