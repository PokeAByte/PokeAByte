import { useEffect, useState } from "react";
import { Button } from "../../../components/Button";
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
			{archivedMappersApi.result && archives.map((archive, index) => {
				return (
					<li key={index} className="row padding surface-container-high">
						<span className={"material-icons blue-text"}> catching_pokemon </span>
						<span>
							{archive.Path}
						</span>
						<span >
							({archive.Mappers.length} files)
						</span>
						<span className="max"></span>
						<Button color="success" onClick={() => setRestoreModal(true)}>
							Restore
						</Button>
						<ConfirmationModal
							display={restoreModal}
							title="Warning"
							confirmLabel="RESTORE!"
							text="Restoring a set of mappers will archive any current copies of those mappers."
							onCancel={() => setRestoreModal(false)}
							onConfirm={() => restoreArchive(archive.Mappers)}
						/>
						<Button color="error" onClick={() => setDeleteModal(true)}>
							Delete
						</Button>
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
	if (!props.display) {
		return null;
	}
	return (
		<>
			<div className="overlay  active"></div>
			<dialog className="modal active no-round">
				{props.title && <h5>{props.title}</h5>}
				<div>{props.text}</div>
				<nav className="right-align no-space">
					<button className="transparent no-round link" onClick={props.onCancel}>
						CANCEL
					</button>
					<button className="transparent no-round link inverse-link" onClick={props.onConfirm}>
						{props.confirmLabel}
					</button>
				</nav>
			</dialog>
		</>
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