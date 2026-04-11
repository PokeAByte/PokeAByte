import { getMappers, MapperFile } from "@/utility/fetch";
import { effect, signal } from "@preact/signals";
import { isConnectedSignal } from "./mapperSignal";
import { getArchivedMappers, getMapperUpdates, MapperArchiveRecord, MapperUpdate } from "@/utility/fetch";


export interface MapperFilesData {
	isLoading: boolean,
	availableMappers: MapperFile[],
	updates: MapperUpdate[],
	archives: MapperArchiveRecord
}

export const mapperFilesSignal = signal<MapperFilesData>({
	availableMappers: [],
	updates: [],
	archives: {}, 
	isLoading: false,
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
	let availableMappers: MapperFile[] = [];
	let updates: MapperUpdate[] = [];
	let archives: MapperArchiveRecord = {};
	
	try {
		availableMappers = await getMappers() ?? [];
		updates = await getMapperUpdates() ?? [];
		archives = await getArchivedMappers() ?? {};
	} finally {
		mapperFilesSignal.value = {
			availableMappers,
			archives, 
			updates,
			isLoading: false
		};
	}
}

