import { ArchivedMapper, MapperVersion } from "pokeaclient";

export async function postWithoutResult<T>(
	requestUrl: string,
	body: null | T = null
) {
	try {
		const response = await fetch(
			requestUrl,
			{
				method: "POST",
				body: JSON.stringify(body),
				headers: { "Content-Type": "application/json" }
			}
		);
		return response.ok;
	} catch {
		return false
	}
}

export async function archiveMappers(mappers: MapperVersion[]) {
	return await postWithoutResult("http://localhost:8085/files/mapper/archive_mappers", mappers);
}

export async function backupMappers(mappers: MapperVersion[]) {
	return await postWithoutResult("http://localhost:8085/files/mapper/backup_mappers", mappers);
}

export async function deleteMappers(mappers: ArchivedMapper[]) {
	return await postWithoutResult("http://localhost:8085/files/mapper/delete_mappers", mappers);
}