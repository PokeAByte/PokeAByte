import { ArchivedMappers, AvailableMapper, MapperUpdate } from "pokeaclient";
import { useEffect, useState } from "preact/hooks";
import { Store } from "../utility/propertyStore";
import { ComponentChildren, createContext } from "preact";

export interface MapperFilesContextData {
	refresh: () => void,
	isLoading: boolean,
	availableMappers: AvailableMapper[],
	updates: MapperUpdate[],
	archives: ArchivedMappers
}

export const MapperFilesContext = createContext<MapperFilesContextData>(null!);

export function MapperFilesContextProvider(props: { children: ComponentChildren}) {
	const refresh = async () => {
		setState({
			...state, 
			isLoading: true,
		});
		const availableMappers = await Store.client.getMappers() ?? [];
		const updates = await Store.client.files.getMapperUpdatesAsync() ?? [];
		const archives = await Store.client.files.getArchivedMappersAsync() ?? {};
		setState({
			...state, 
			availableMappers,
			archives, 
			updates,
			isLoading: false
		});
	}
	const [state, setState] = useState<MapperFilesContextData>({
		refresh,
		availableMappers: [],
		updates: [],
		archives: {}, 
		isLoading: true,
	});
	useEffect(() => {
		refresh();
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, []) // We only want to run the effect once on mount. 

	return (
		<MapperFilesContext.Provider value={state} >
			{props.children}
		</MapperFilesContext.Provider>
	)
}