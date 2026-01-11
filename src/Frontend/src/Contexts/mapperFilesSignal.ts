import { ArchivedMappers, AvailableMapper, MapperUpdate } from "pokeaclient";
import { Store } from "../utility/propertyStore";
import { effect, signal } from "@preact/signals";
import { isConnectedSignal } from "./mapperSignal";


export interface MapperFilesData {
	isLoading: boolean,
	availableMappers: AvailableMapper[],
	updates: MapperUpdate[],
	archives: ArchivedMappers
}

export const mapperFilesSignal = signal<MapperFilesData>({
	availableMappers: [],
	updates: [],
	archives: {}, 
	isLoading: true,
});

effect(() => {
	if (isConnectedSignal.value) {
		refreshMapperFiles();
	}
})

export async function refreshMapperFiles() {
	mapperFilesSignal.value = {
		...mapperFilesSignal.peek(),
		isLoading: true,
	};
	const availableMappers = await Store.client.getMappers() ?? [];
	const updates = await Store.client.files.getMapperUpdatesAsync() ?? [];
	const archives = await Store.client.files.getArchivedMappersAsync() ?? {};
	mapperFilesSignal.value = {
		availableMappers,
		archives, 
		updates,
		isLoading: false
	};
}

