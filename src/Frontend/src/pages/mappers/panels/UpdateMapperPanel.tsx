import { LoadProgress } from "../../../components/LoadProgress";
import { MapperSelectionTable } from "./components/MapperSelectionTable";
import { useAPI } from "../../../hooks/useAPI";
import { mapperFilesSignal, refreshMapperFiles } from "../../../Contexts/mapperFilesSignal";
import { useEffect, useState } from "preact/hooks";
import { OpenMapperFolderButton } from "../../../components/OpenMapperFolderButton";
import { Toasts } from "../../../notifications/ToastStore";
import { WideButton } from "../../../components/WideButton";
import { Panel } from "@/components/Panel";
import { Show } from "@preact/signals/utils";
import { advancedModeSignal } from "@/Contexts/uiSettingsSignal";
import { installMapper, MapperUpdate } from "@/utility/fetch";

export function UpdateMapperPanel() {
	const mapperFiles = mapperFilesSignal.value;
	const [availableUpdates, setAvailableUpdates] = useState<MapperUpdate[]>([]);
	const [selectedUpdates, sectSelectedUpdates] = useState<string[]>([]);
	const downloadMappers = useAPI(
		installMapper, 
		(_, success) => {
			if (success) {
				refreshMapperFiles();
				Toasts.push(`Successfully update mapper(s).`, "task_alt", "green");
			}
		}
	);

	useEffect(() => {
		setAvailableUpdates(
			mapperFiles.updates
				.filter(mapper => mapper.version)
				.filter(mapper => mapper.remote_version != mapper.version)
		);
		sectSelectedUpdates([]);
	}, [mapperFiles.updates])

	useEffect(() => {
		if (downloadMappers.wasCalled && !downloadMappers.isLoading) {
			sectSelectedUpdates([]);
			downloadMappers.reset();
		}
	}, [downloadMappers, downloadMappers.wasCalled, downloadMappers.isLoading])

	const handleUpdate = () => {
		const mappers = availableUpdates.filter(x => selectedUpdates.includes(x.path));
		downloadMappers.call(mappers.map(x => x.path));
	}

	const handleUpdateAll = () => {
		downloadMappers.call(availableUpdates.map(x => x.path));
	}

	if (downloadMappers.isLoading || mapperFiles.isLoading) {
		return <LoadProgress label="Downloading mapper(s)" />
	}

	return (
		<Panel id="mapper-update" title="Update mappers" >
			<span>
				{selectedUpdates.length} / {availableUpdates.length} Mappers Selected
			</span>
			<div class="flexy-panel margin-top">
				<WideButton text="Update selected" color="green" disabled={!selectedUpdates.length} onClick={handleUpdate} />
				<WideButton text="Update all" color="green" disabled={!availableUpdates.length} onClick={handleUpdateAll} />
				<WideButton text="Check for updates" color="blue" onClick={refreshMapperFiles} />
				<Show when={advancedModeSignal}>
					<OpenMapperFolderButton />
				</Show>
			</div>
			<MapperSelectionTable
				availableMappers={availableUpdates}
				selectedMappers={selectedUpdates}
				onMapperSelection={sectSelectedUpdates}
				installedHeader="Current"
				availableHeader="New"
				fallback={
					mapperFiles.availableMappers.length == 0
						? <strong>No mappers installed. </strong>
						: <strong>All installed mappers ({mapperFiles.availableMappers.length}) are up to date.</strong>
			}
			/>
		</Panel>
	);
}
