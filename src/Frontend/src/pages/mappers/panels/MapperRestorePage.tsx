import { useState } from "preact/hooks";
import { useAPI } from "../../../hooks/useAPI";
import { ConfirmationModal } from "../../../components/ConfirmationModal";
import { mapperFilesSignal, refreshMapperFiles } from "../../../Contexts/mapperFilesSignal";
import { OpenMapperFolderButton } from "../../../components/OpenMapperFolderButton";
import { WideButton } from "../../../components/WideButton";
import { Panel } from "@/components/Panel";
import { Show } from "@preact/signals/utils";
import { advancedModeSignal } from "@/Contexts/uiSettingsSignal";
import { Icon } from "@/components/Icon";
import { deleteArchive, MapperArchive, MapperArchiveRecord, openMapperFolder, restoreMapper } from "@/utility/fetch";

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
							archive={archive}
							restoreArchive={restoreArchiveApi.call}
							deleteArchive={deleteArchiveApi.call}
						/>
					);
				})}
			</ul>
		</Panel>
	);
}

type MapperRestoreRowProps = {
	archive: Archive,
	restoreArchive: (mappers: string) => void,
	deleteArchive: (mappers: string) => void,
}

export function MapperRestoreRow(props: MapperRestoreRowProps) {
	const { archive, restoreArchive, deleteArchive } = props;
	const [restoreModal, setRestoreModal] = useState(false);
	const [deleteModal, setDeleteModal] = useState(false);
	return (
		<li class="margin-top">
			<details>
				<summary>
					<Icon name="catching_pokemon" />
					<span>
						{archive.Path} ({archive.Mappers.length} files)
					</span>
					<span class="flexy-panel">
						<WideButton text="Restore" color="green" onClick={() => setRestoreModal(true)} />
						<WideButton text="Delete" color="red" onClick={() => setDeleteModal(true)} />
					</span>
				</summary>
				<div>
					<ul>
						{archive.Mappers.map(archivedMapper =>
							<li key={archivedMapper.path + archivedMapper.mapper.path}>
								{archivedMapper.path}/{archivedMapper.mapper.display_name}
								&nbsp;
								<i>({archivedMapper.mapper.version})</i>
							</li>
						)}
					</ul>
				</div>
			</details>
			<div>
			</div>
			<ConfirmationModal
				display={restoreModal}
				title="Warning"
				confirmLabel="RESTORE!"
				text={
					<>
						<p>
							Restoring a set of mappers will archive any current copies of those mappers.
							<br />Do you want to restore the following files?
						</p>
						<p>{archive.Path}</p>
						<ul>
							{archive.Mappers.map(archive =>
								<li key={archive.path}>
									<span>{archive.mapper.path} version {archive.mapper.version}</span>
								</li>
							)}
						</ul>
					</>
				}
				onCancel={() => setRestoreModal(false)}
				onConfirm={() => restoreArchive(archive.Path)}
			/>
			<ConfirmationModal
				display={deleteModal}
				title="Warning"
				confirmLabel="DELETE!"
				text={
					<>
						<p>
							Deleting a set of archived mappers <strong>cannot be undone</strong>. Proceed with caution.
							<br />Do you want to delete the following files?
						</p>
						<p>{archive.Path}</p>
						<ul>
							{archive.Mappers.map(x =>
								<li key={x.path}>
									<span key={x.path}>{x.path}{x.mapper.display_name}</span>
								</li>
							)}
						</ul>
					</>
				}
				onCancel={() => setDeleteModal(false)}
				onConfirm={() => deleteArchive(archive.Path)}
			/>
		</li>
	)

}

type Archive = {
	Path: string,
	Mappers: MapperArchive[],
}

function processArchive(mappers: MapperArchiveRecord | null) {
	if (!mappers) {
		return [];
	}
	return Object.keys(mappers).reduce<Archive[]>(
		(accumulator, key) => {
			const currentPath = key;
			const existingBucket = accumulator.find(x => x.Path === currentPath);
			if (existingBucket) {
				existingBucket.Mappers.push(...mappers[key])
			} else {
				const newBucket: Archive = {
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