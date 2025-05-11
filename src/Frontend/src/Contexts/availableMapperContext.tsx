import { ArchivedMappers, AvailableMapper, MapperUpdate } from "pokeaclient";
import React, { useEffect, useState } from "react";
import { Store } from "../utility/propertyStore";

export interface MapperFilesContextData {
	refresh: () => void,
	isLoading: boolean,
	availableMappers: AvailableMapper[],
	updates: MapperUpdate[],
	archives: ArchivedMappers
}

export const MapperFilesContext = React.createContext<MapperFilesContextData>(null!);

export function MapperFilesContextProvider(props: { children: React.ReactNode}) {
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
	}, [])

	return (
		<MapperFilesContext.Provider value={state} >
			{props.children}
		</MapperFilesContext.Provider>
	)
}