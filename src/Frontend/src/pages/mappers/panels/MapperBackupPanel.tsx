import { useState, useEffect } from "preact/hooks";
import { LoadProgress } from "@/components/LoadProgress";
import { MapperSelectionTable } from "./components/MapperSelectionTable";
import { useAPI } from "@/hooks/useAPI";
import { archiveMappers, backupMappers, MapperUpdate, openArchiveFolder } from "@/utility/fetch";
import { mapperFilesSignal, refreshMapperFiles } from "@/Contexts/mapperFilesSignal";
import { OpenMapperFolderButton } from "@/components/OpenMapperFolderButton";
import { WideButton } from "@/components/WideButton";
import { Panel } from "@/components/Panel";
import { Show } from "@preact/signals/utils";
import { advancedModeSignal } from "@/Contexts/uiSettingsSignal";

export function MapperBackupPanel() {
	const mapperFiles = mapperFilesSignal.value;
	const [availableMappers, setAvailableMappers] = useState<MapperUpdate[]>([]);
	const [selectedMappers, setSelectedMappers] = useState<string[]>([]);
	const archiveMappersApi = useAPI(archiveMappers, refreshMapperFiles);
	const backupApi = useAPI(backupMappers, refreshMapperFiles);
	// Load available mappers:

	// Process loaded mappers:
	useEffect(() => {
		setAvailableMappers(mapperFiles.updates.filter(mapper => !!mapper.version) ?? []);
	}, [mapperFiles.updates])

	const handleArchiveSelected = () => {
		const mappers = availableMappers
			.filter(x => selectedMappers.includes(x.path))
			.map(x => x.path);
		setSelectedMappers([]);
		archiveMappersApi.call(mappers);
	}
	const handleArchiveAll = () => {
		archiveMappersApi.call(availableMappers.map(x => x.path));
	}
	const handleBackupSelected = () => {
		const mappers = availableMappers
			.filter(x => selectedMappers.includes(x.path))
			.map(x => x.path);
		setSelectedMappers([]);
		backupApi.call(mappers);
	}
	const handleBackupAll = () => {
		backupApi.call(availableMappers.map(x => x.path));
	}

	if (mapperFiles.isLoading) {
		return <LoadProgress label="Processing mapper(s)" />
	}
	return (
		<Panel id="mapper-backup" title="Backup mappers" >		
			<span>{selectedMappers.length} / {availableMappers.length} Mappers Selected</span>
			<div class="flexy-panel margin-top">
				<WideButton 
					text="Backup selected" 
					disabled={!selectedMappers.length} 
					onClick={handleBackupSelected} 
					color="green" 
				/>
				<WideButton 
					text="Backup all" 
					disabled={!availableMappers.length} 
					onClick={handleBackupAll} 
					color="green" 
				/>
				<Show when={advancedModeSignal}>
					<OpenMapperFolderButton />
				</Show>
			</div>
			<div class="flexy-panel margin-top">
				<WideButton 
					text="Archive selected" 
					disabled={selectedMappers.length === 0} 
					onClick={handleArchiveSelected} 
					color="red" 
				/>
				<WideButton 
					color="red" 
					disabled={availableMappers.length === 0} 
					onClick={handleArchiveAll}
					text="Archive all"
				/>
				<Show when={advancedModeSignal}>
					<WideButton 
						color="purple" 
						onClick={openArchiveFolder}
						text="Open archive folder"
					/>
				</Show>
			</div>
			<div class="margin-top">
				{archiveMappersApi.isLoading
					? <LoadProgress label="Archiving mapper(s)" />
					: <MapperSelectionTable
						availableMappers={availableMappers}
						selectedMappers={selectedMappers}
						onMapperSelection={setSelectedMappers}
						onUpdateList={refreshMapperFiles}
						installedHeader="Version"
						fallback={<strong>No mappers installed.</strong>}
					/>
				}
			</div>
		</Panel>
	);
}
