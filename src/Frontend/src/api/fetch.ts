import { AppSettingsModel, GitHubSettings, MapperArchiveRecord, MapperFile, MapperUpdate } from "./types";
import { fetchGet, fetchWithBody, postWithoutResult } from "./fetchFunctions";

export async function changeMapper(mapperId: string | null) {
	return fetchWithBody("PUT", "/mapper-service/change-mapper", mapperId);
}

export async function getDriverName() {
	return await fetchGet<string>("/driver/name");
}

export async function archiveMappers(mappers: string[]) {
	return await postWithoutResult("/files/mapper/archive_mappers", mappers);
}

export async function backupMappers(mappers: string[]) {
	return await postWithoutResult("/files/mapper/backup_mappers", mappers);
}

export async function getAppSettings<AppSettings>() {
	return fetchGet<AppSettings>("/settings/appsettings");
}

export async function saveAppSettings(settings: Partial<AppSettingsModel>) {
	return await postWithoutResult("/settings/save_appsettings", settings);
}

export async function resetAppSettings() {
	return await postWithoutResult("/settings/appsettings/reset");
}

export const getMappers = () =>fetchGet<MapperFile[]>("/mapper-service/get-mappers");

export async function getMapperUpdates(force: boolean = false) {
	if (force) {
		await fetchGet<boolean>("/files/mapper/check_for_updates");
	}
	return await fetchGet<MapperUpdate[]>("/files/mapper/get_updates");
}

export async function getArchivedMappers() {
	return await fetchGet<MapperArchiveRecord>("/files/mapper/get_archived");
}

export const openMapperFolder = () => fetchGet<null>("/files/open_mapper_folder");

export const openArchiveFolder = () => fetchGet<null>("/files/open_mapper_archive_folder");

export const getGithubSettings = () => fetchGet<GitHubSettings>("/files/get_github_settings");

export const saveGitHubSettings = (settings: Partial<GitHubSettings>) => fetchWithBody("POST", "/files/save_github_settings", settings);

export const installMapper = (paths: string[]) => fetchWithBody("POST", "/files/mapper/download_updates", paths)

export const deleteArchive = (path: string) => fetchWithBody("POST", "/files/mapper/delete_mappers", path)

export const restoreMapper = (path: string) => fetchWithBody("POST", "/files/mapper/restore_mappers", path)

export const getGithubLink = () => fetchGet<string>("/files/get_github_link");

export const testGithubSettings = () => fetchGet<string>("/files/save_github_settings");