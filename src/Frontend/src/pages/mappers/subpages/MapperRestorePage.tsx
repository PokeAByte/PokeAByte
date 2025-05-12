import { Button } from "../../../components/Button";
import { useEffect, useRef, useState } from "react";
import { useAPI } from "../../../hooks/useAPI";
import { Store } from "../../../utility/propertyStore";
import { ArchivedMapper, ArchivedMappers } from "pokeaclient";


export function MapperRestorePage() {
	const filesClient = Store.client.files;
	const [restoreModal, setRestoreModal] = useState(false);
	const [deleteModal, setDeleteModal] = useState(false);
	const archivedMappersApi = useAPI(filesClient.getArchivedMappersAsync);
	const deleteArchiveApi = useAPI(filesClient.deleteMappers, archivedMappersApi.call);
	const restoreArchiveApi = useAPI(filesClient.restoreMapper, archivedMappersApi.call);
	const archives = processArchive(archivedMappersApi.result);

	useEffect(
		() => archivedMappersApi.call(),
		// eslint-disable-next-line react-hooks/exhaustive-deps
		[]
	);

	const restoreArchive = (mappers: ArchivedMapper[]) => {
		setRestoreModal(false);
		restoreArchiveApi.call(mappers);
	}

	const deleteArchive = (mappers: ArchivedMapper[]) => {
		setDeleteModal(false);
		deleteArchiveApi.call(mappers);
	}

	return (
		<div className="padding-top">
			<h5 className="small ">
				{archives.length} Archives/Backups and {archives.reduce((c, x) => c + x.Mappers.length, 0)} files found
			</h5>
			<div className="row wrap">
				<Button color="blue" onClick={filesClient.openMapperFolder}>OPEN MAPPER FOLDER</Button>
				<Button color="blue" onClick={filesClient.openMapperFolder}>OPEN ARCHIVE/BACKUP FOLDER</Button>
			</div>
			<div className="row margin-top">
				<button className="border-blue margin-right" type="button" onClick={filesClient.openMapperFolder}>
					OPEN MAPPER FOLDER
				</button>
				<button className="border-blue" type="button" onClick={filesClient.openMapperFolder}>
					OPEN ARCHIVE/BACKUP FOLDER
				</button>
			</div>
			<br />
			<ul className="mapper-archives">
				{archivedMappersApi.result && archives.map((archive, index) => {
					return (
						<li key={index} className="margin-top">
							<details>
								<summary>
									<span className={"material-icons"}> catching_pokemon </span>
									<span>
										{archive.Path} ({archive.Mappers.length} files)
									</span>
									<span>
										<button type="button" className="border-green margin-right" onClick={() => setRestoreModal(true)}>
											Restore
										</button>
										<button type="button" className="border-red" onClick={() => setDeleteModal(true)}>
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
								text="Restoring a set of mappers will archive any current copies of those mappers."
								onCancel={() => setRestoreModal(false)}
								onConfirm={() => restoreArchive(archive.Mappers)}
							/>
							<ConfirmationModal
								display={deleteModal}
								title="Warning"
								confirmLabel="DELETE!"
								text="Deleting a set of archived mappers cannot be undone. Proceed with caution."
								onCancel={() => setDeleteModal(false)}
								onConfirm={() => deleteArchive(archive.Mappers)}
							/>
						</li>
					);
				})}
			</ul>
		</div>
	);
}

type ModalProps = {
	display: boolean,
	title?: string,
	text: string,
	confirmLabel: string,
	onConfirm: () => void,
	onCancel: () => void,
}

function ConfirmationModal(props: ModalProps) {
	const dialogRef = useRef<HTMLDialogElement>(null);
	useEffect(() => {
		if (!!dialogRef.current && props.display) {
			dialogRef.current.showModal();
		}
	}, [!!dialogRef.current, props.display]);
	if (!props.display) {
		return null;
	}
	return (
		<dialog ref={dialogRef} onToggle={(e) => e.newState === "closed" && props.onCancel()}>
			{props.title && <h2>{props.title}</h2>}
			<p>{props.text}</p>
			<div>
				<button className="margin-right" onClick={props.onCancel}>
					CANCEL
				</button>
				<button className="" onClick={props.onConfirm}>
					{props.confirmLabel}
				</button>
			</div>
		</dialog>

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