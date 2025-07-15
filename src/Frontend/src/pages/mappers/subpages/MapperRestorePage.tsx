import { useState, useContext } from "preact/hooks";
import { useAPI } from "../../../hooks/useAPI";
import { Store } from "../../../utility/propertyStore";
import { ArchivedMapper, ArchivedMappers } from "pokeaclient";
import { ConfirmationModal } from "./ConfirmationModal";
import { MapperFilesContext } from "../../../Contexts/availableMapperContext";
import { OpenMapperFolderButton } from "../../../components/OpenMapperFolderButton";
import { Advanced } from "../../../Contexts/Advanced";

export function MapperRestorePage() {
	const filesClient = Store.client.files;
	const mapperFileContext = useContext(MapperFilesContext);
	const deleteArchiveApi = useAPI(filesClient.deleteMappers, mapperFileContext.refresh);
	const restoreArchiveApi = useAPI(filesClient.restoreMapper, mapperFileContext.refresh);
	const archives = processArchive(mapperFileContext.archives);

	return (
		<div>
			<div class="margin-top">
				<strong>
					{archives.length} Archives/Backups and {archives.reduce((c, x) => c + x.Mappers.length, 0)} files found
				</strong>
			</div>
			<Advanced>
				<div className="row margin-top">
					<OpenMapperFolderButton />
					<button className="margin-left blue wide-button" type="button" onClick={filesClient.openMapperFolder}>
						Open archive/backup folder
					</button>
				</div>
				<br />
			</Advanced>
			<ul className="mapper-archives margin-top">
				{archives.map((archive) => {
					return (
						<MapperRestoreRow
							key={archive.Path}
							archive={archive}
							restoreArchive={restoreArchiveApi.call}
							deleteArchive={deleteArchiveApi.call}
						/>
					);
				})}
			</ul>
		</div>
	);
}

type MapperRestoreRowProps = {
	archive: Archive,
	restoreArchive: (mappers: ArchivedMapper[]) => void,
	deleteArchive: (mappers: ArchivedMapper[]) => void,
}

export function MapperRestoreRow(props: MapperRestoreRowProps) {
	const { archive, restoreArchive, deleteArchive } = props;
	const [restoreModal, setRestoreModal] = useState(false);
	const [deleteModal, setDeleteModal] = useState(false);
	return (
		<li className="margin-top">
			<details>
				<summary>
					<span className={"material-icons"}> catching_pokemon </span>
					<span>
						{archive.Path} ({archive.Mappers.length} files)
					</span>
					<span>
						<button type="button" className="wide-button green margin-right" onClick={() => setRestoreModal(true)}>
							Restore
						</button>
						<button type="button" className="wide-button red" onClick={() => setDeleteModal(true)}>
							Delete
						</button>
					</span>
				</summary>
				<div>
					<ul>
						{archive.Mappers.map(archivedMapper =>
							<li key={archivedMapper.fullPath}>
								{archivedMapper.pathDisplayName}/{archivedMapper.mapper.display_name}
								&nbsp;
								<i>({archivedMapper.mapper.date_created})</i>
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
							{archive.Mappers.map(x =>
								<li key={x.pathDisplayName}>
									<span key={x.pathDisplayName}>{x.pathDisplayName}{x.mapper.display_name}</span>
								</li>
							)}
						</ul>
					</>
				}
				onCancel={() => setRestoreModal(false)}
				onConfirm={() => restoreArchive(archive.Mappers)}
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
								<li key={x.pathDisplayName}>
									<span key={x.pathDisplayName}>{x.pathDisplayName}{x.mapper.display_name}</span>
								</li>
							)}
						</ul>
					</>
				}
				onCancel={() => setDeleteModal(false)}
				onConfirm={() => deleteArchive(archive.Mappers)}
			/>
		</li>
	)

}

type Archive = {
	Path: string,
	Mappers: ArchivedMapper[],
}

function processArchive(mappers: ArchivedMappers | null) {
	if (!mappers) {
		return [];
	}
	return Object.keys(mappers).reduce<Archive[]>(
		(accumulator, key) => {
			const currentPath = extractMapperPath(key);
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

function extractMapperPath(path: string) {
	const fragments = path.split("/");
	const mapperIndex = fragments.length - 3;
	return "/" + fragments[mapperIndex];
}