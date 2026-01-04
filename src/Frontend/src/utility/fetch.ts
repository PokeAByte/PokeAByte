import { MapperVersion } from "pokeaclient";

const BASE_URL = "http://localhost:8085";
const DEFAULT_HEADERS = { "Content-Type": "application/json" };

async function postWithoutResult<T>(requestUrl: string, body: null | T = null) {
	try {
		const response = await fetch(
			requestUrl,
			{
				method: "POST",
				body: JSON.stringify(body),
				headers: DEFAULT_HEADERS,
			}
		);
		return response.ok;
	} catch {
		return false
	}
}

export async function changeMapper(mapperId: string | null) {
	try {
		const response = await fetch(
			BASE_URL + "/mapper-service/change-mapper",
			{
				method: "PUT",
				body: JSON.stringify(mapperId),
				headers: DEFAULT_HEADERS,
			}
		);
		return response.ok || await response.json() as string;
	} catch {
		return false;
	}
}

export async function archiveMappers(mappers: MapperVersion[]) {
	return await postWithoutResult(BASE_URL + "/files/mapper/archive_mappers", mappers);
}

export async function backupMappers(mappers: MapperVersion[]) {
	return await postWithoutResult(BASE_URL + "/files/mapper/backup_mappers", mappers);
}
