import { Store } from "../../../utility/propertyStore"
import { LoadProgress } from "../../../components/LoadProgress";
import { MapperSelectionTable } from "./components/MapperSelectionTable";
import { useAPI } from "../../../hooks/useAPI";
import { MapperUpdate } from "pokeaclient";
import { MapperFilesContext } from "../../../Contexts/availableMapperContext";
import { useContext, useEffect, useState } from "preact/hooks";
import { OpenMapperFolderButton } from "../../../components/OpenMapperFolderButton";
import { Toasts } from "../../../notifications/ToastStore";
import { Advanced } from "../../../components/Advanced";
import { WideButton } from "../../../components/WideButton";
import { Panel } from "@/components/Panel";

export function UpdateMapperPanel() {
	const filesClient = Store.client.files;
	const mapperFileContext = useContext(MapperFilesContext);
	const [availableUpdates, setAvailableUpdates] = useState<MapperUpdate[]>([]);
	const [selectedUpdates, sectSelectedUpdates] = useState<string[]>([]);
	const downloadMappers = useAPI(
		filesClient.downloadMapperUpdatesAsync,
		(success) => {
			if (success) {
				mapperFileContext.refresh();
				Toasts.push(`Successfully update mapper(s).`, "task_alt", "green");
			} else {
				Toasts.push(`An error occured while updating.`, "", "red");
			}
		}
	);

	useEffect(() => {
		setAvailableUpdates(
			mapperFileContext.updates
				.filter(mapper => !!mapper.currentVersion)
				.filter(mapper => !!mapper.latestVersion)
		);
		sectSelectedUpdates([]);
	}, [mapperFileContext.updates])

	useEffect(() => {
		if (downloadMappers.wasCalled && !downloadMappers.isLoading) {
			sectSelectedUpdates([]);
			downloadMappers.reset();
		}
	}, [downloadMappers, downloadMappers.wasCalled, downloadMappers.isLoading])

	const handleUpdate = () => {
		const mappers = availableUpdates.filter(x => selectedUpdates.includes(x.latestVersion.path));
		downloadMappers.call(mappers);
	}

	const handleUpdateAll = () => {
		downloadMappers.call(availableUpdates);
	}

	if (downloadMappers.isLoading || mapperFileContext.isLoading) {
		return <LoadProgress label="Downloading mapper(s)" />
	}

	return (
		<Panel id="mapper-update" title="Update mappers" >
			<span>
				{selectedUpdates.length} / {availableUpdates.length} Mappers Selected
			</span>
			<div class="margin-top">
				<WideButton text="Update selected" color="green" disabled={!selectedUpdates.length} onClick={handleUpdate} />
				<WideButton text="Update all" color="green" disabled={!availableUpdates.length} onClick={handleUpdateAll} />
				<WideButton text="Reload mapper list" color="blue" onClick={mapperFileContext.refresh} />
				<Advanced>
					<OpenMapperFolderButton />
				</Advanced>
			</div>
			<MapperSelectionTable
				availableMappers={availableUpdates}
				selectedMappers={selectedUpdates}
				onMapperSelection={sectSelectedUpdates}
			/>
		</Panel>
	);
}
