export type AppSettingsModel = {
	RETROARCH_LISTEN_IP_ADDRESS: string;
	RETROARCH_LISTEN_PORT: number;
	RETROARCH_READ_PACKET_TIMEOUT_MS: number;
	DELAY_MS_BETWEEN_READS: number;
	PROTOCOL_FRAMESKIP: number;
};

export type MapperFile = {
	display_name: string,
	path: string,
	version?: string,
}

export type MapperUpdate = MapperFile & { remote_version : string};

export type MapperArchive = {
	path: string,
	mapper: MapperFile,
}

export type MapperArchiveRecord = Record<string, MapperArchive[]>;

export type GitHubSettings = {
	owner: string,
	repo: string,
	dir: string,
	token: string,
	accept: string,
	api_version: string,
}