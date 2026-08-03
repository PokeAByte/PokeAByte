import { useAPI } from "../../../hooks/useAPI";
import { mapperFilesSignal, refreshMapperFiles } from "../../../Contexts/mapperFilesSignal";
import { OpenMapperFolderButton } from "../../../components/OpenMapperFolderButton";
import { WideButton } from "../../../components/WideButton";
import { Panel } from "@/components/Panel";
import { Show } from "@preact/signals/utils";
import { advancedModeSignal } from "@/Contexts/uiSettingsSignal";
import { deleteArchive,  openMapperFolder, restoreMapper } from "@/api/fetch";
import { MapperRestoreRow } from "./components/MapperRestoreRow";
import { MapperArchive, MapperArchiveRecord } from "@/api/types";

export function RestoreMapperPanel() {
	const mapperFiles = mapperFilesSignal.value;
	const deleteArchiveApi = useAPI(deleteArchive, refreshMapperFiles);
	const restoreArchiveApi = useAPI(restoreMapper, refreshMapperFiles);
	const archives = processArchive(mapperFiles.archives);

	return (
		<Panel id="mapper-restore" title="Restore backup/archive" >
			<div class="margin-top">
				<strong>
					{archives.length} Archives/Backups and {archives.reduce((c, x) => c + x.Mappers.length, 0)} files found
				</strong>
			</div>
			<Show when={advancedModeSignal}>
				<div class="flexy-panel margin-top">
					<OpenMapperFolderButton />
					<WideButton color="blue" onClick={openMapperFolder} text="Open archive folder" />
				</div>
				<br />
			</Show>
			<ul class="mapper-archives margin-top">
				{archives.map((archive) => {
					return (
						<MapperRestoreRow
							key={archive.Path + "" + archive.Mappers.length}
							folder={archive}
							restoreArchive={restoreArchiveApi.call}
							deleteArchive={deleteArchiveApi.call}
						/>
					);
				})}
			</ul>
		</Panel>
	);
}

export type MapperRestoreRowProps = {
	folder: ArchiveFolder,
	restoreArchive: (mappers: string) => void,
	deleteArchive: (mappers: string) => void,
}

type ArchiveFolder = {
	Path: string,
	Mappers: MapperArchive[],
}

function processArchive(mappers: MapperArchiveRecord | null) {
	if (!mappers) {
		return [];
	}
	return Object.keys(mappers).reduce<ArchiveFolder[]>(
		(accumulator, key) => {
			const currentPath = key;
			const existingBucket = accumulator.find(x => x.Path === currentPath);
			if (existingBucket) {
				existingBucket.Mappers.push(...mappers[key])
			} else {
				const newBucket: ArchiveFolder = {
					Path: currentPath,
					Mappers: [...mappers[key]]
				}
				accumulator.push(newBucket);
			}
			return accumulator;
		},
		[]
	);
}