import { useEffect,  useState } from "preact/hooks";
import { Store } from "../../../utility/propertyStore";
import { GameProperty } from "pokeaclient";
import { subscribePath } from "./subscribePaths";

export function useGameProperty<T>(path: string) {
	const [property, setProperty] = useState<GameProperty<T>|null>(() => Store.getProperty(path));
	useEffect(
		() => {
			const callback = () => {
				setProperty(Store.getProperty(path))
			};
			return subscribePath(path, callback);
		},
		[path]
	);
	return property;
}
