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

type Serializable = Record<string, string|number> | string[] | string | number | null;

export async function fetchWithBody<T extends Serializable>(method: "PUT"|"POST", requestUrl: string, data: T) {
	try {
		const response = await fetch(
			BASE_URL + requestUrl,
			{
				method: method,
				body: JSON.stringify(data),
				headers: DEFAULT_HEADERS,
			}
		);
		return response.ok || await response.json() as string;
	} catch {
		return false;
	}
}

export async function fetchGet<T>(requestUrl: string): Promise<T|null> {
	try {
		const response = await fetch(
			BASE_URL + requestUrl,
			{
				method: "GET",
				headers: DEFAULT_HEADERS,
			}
		);
		return response.ok 
			? await response.json() as T
			: null;
	} catch {
		return null;
	}
}

export async function changeMapper(mapperId: string | null) {
	return fetchWithBody("PUT", "/mapper-service/change-mapper", mapperId);
}

export async function getDriverName() {
	return await fetchGet<string>("/driver/name");
}

export async function archiveMappers(mappers: string[]) {
	return await postWithoutResult(BASE_URL + "/files/mapper/archive_mappers", mappers);
}

export async function backupMappers(mappers: string[]) {
	return await postWithoutResult(BASE_URL + "/files/mapper/backup_mappers", mappers);
}

export type AppSettingsModel = {
	RETROARCH_LISTEN_IP_ADDRESS: string,
	RETROARCH_LISTEN_PORT: number,
	RETROARCH_READ_PACKET_TIMEOUT_MS: number,
	DELAY_MS_BETWEEN_READS: number,
	PROTOCOL_FRAMESKIP: number,
}

export async function getAppSettings<AppSettings>() {
	try {
		const response = await fetch(
			BASE_URL + "/settings/appsettings",
			{ headers: DEFAULT_HEADERS }
		);
		return <AppSettings>response.json();
	} catch {
		throw new Error("Unable to retrieve app settings.");
	}
}

export async function saveAppSettings(settings: Partial<AppSettingsModel>) {
	return await postWithoutResult(BASE_URL + "/settings/save_appsettings", settings);
}

export async function resetAppSettings() {
	return await postWithoutResult(BASE_URL + "/settings/appsettings/reset");
}

export type MapperFile = {
	display_name: string,
	path: string,
	version?: string,
}

export type MapperUpdate = MapperFile & { remote_version : string};
export const getMappers = () =>fetchGet<MapperFile[]>("/mapper-service/get-mappers");

export async function getMapperUpdates() {
	return await fetchGet<MapperUpdate[]>("/files/mapper/get_updates");
}
export type MapperArchive = {
	path: string,
	mapper: MapperFile,
}

export type MapperArchiveRecord = Record<string, MapperArchive[]>;

export async function getArchivedMappers() {
	return await fetchGet<MapperArchiveRecord>("/files/mapper/get_archived");
}

export const openMapperFolder = () => fetchGet<null>("/files/open_mapper_folder");
export const openArchiveFolder = () => fetchGet<null>("/files/open_mapper_archive_folder");

export type GitHubSettings = {
	owner: string,
	repo: string,
	dir: string,
	token: string,
	accept: string,
	api_version: string,
}

export const getGithubSettings = () => fetchGet<GitHubSettings>("/files/get_github_settings");

export const saveGitHubSettings = (settings: Partial<GitHubSettings>) => fetchWithBody("POST", "/files/save_github_settings", settings);

export const installMapper = (paths: string[]) => fetchWithBody("POST", "/files/mapper/download_updates", paths)
export const deleteArchive = (path: string) => fetchWithBody("POST", "/files/mapper/delete_mappers", path)
export const restoreMapper = (path: string) => fetchWithBody("POST", "/files/mapper/restore_mappers", path)
export const getGithubLink = () => fetchGet<string>("/files/get_github_link");
export const testGithubSettings = () => fetchGet<string>("/files/save_github_settings");