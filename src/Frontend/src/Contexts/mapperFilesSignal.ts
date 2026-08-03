import { getMappers } from "@/api/fetch";
import { effect, signal } from "@preact/signals";
import { isConnectedSignal } from "./mapperSignal";
import { getArchivedMappers, getMapperUpdates } from "@/api/fetch";
import { MapperArchiveRecord, MapperFile, MapperUpdate } from "@/api/types";


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

export async function refreshMapperFiles(forceUpdateCheck: boolean = false) {
	mapperFilesSignal.value = {
		...mapperFilesSignal.peek(),
		isLoading: true,
	};
	let availableMappers: MapperFile[] = [];
	let updates: MapperUpdate[] = [];
	let archives: MapperArchiveRecord = {};
	
	try {
		availableMappers = await getMappers() ?? [];
		updates = await getMapperUpdates(forceUpdateCheck) ?? [];
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

