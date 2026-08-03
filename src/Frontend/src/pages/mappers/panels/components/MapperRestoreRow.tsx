import { ConfirmationModal } from "@/components/ConfirmationModal";
import { Icon } from "@/components/Icon";
import { WideButton } from "@/components/WideButton";
import { useState } from "preact/hooks";
import { MapperRestoreRowProps } from "../MapperRestorePage";
import { ArchivedMapperListItem } from "./ArchivedMapperListItem";

export function MapperRestoreRow(props: MapperRestoreRowProps) {
	const { folder: archive, restoreArchive, deleteArchive } = props;
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
							<ArchivedMapperListItem key={archivedMapper.mapper.path} item={archivedMapper} />
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
				onCancel={() => setRestoreModal(false)}
				onConfirm={() => restoreArchive(archive.Path)}
			>
				<p>
					Restoring a set of mappers will archive any current copies of those mappers.
					<br />Do you want to restore the following files?
				</p>
				<p>{archive.Path}</p>
				<ul>
					{archive.Mappers.map(archive => 
						<ArchivedMapperListItem key={archive.mapper.path} item={archive} />
					)}
				</ul>
			</ConfirmationModal>
			<ConfirmationModal
				display={deleteModal}
				title="Warning"
				confirmLabel="DELETE!"
				onCancel={() => setDeleteModal(false)}
				onConfirm={() => deleteArchive(archive.Path)}
			>
				<p>
					Deleting a set of archived mappers <strong>cannot be undone</strong>. Proceed with caution.
					<br />Do you want to delete the following files?
				</p>
				<p>{archive.Path}</p>
				<ul>
					{archive.Mappers.map(archivedMapper => 
						<ArchivedMapperListItem key={archivedMapper.mapper.path} item={archivedMapper} />
					)}
				</ul>
			</ConfirmationModal>
		</li>
	);

}
