import { Store } from "@/utility/propertyStore"
import { useState, useContext, useEffect } from "preact/hooks";
import { LoadProgress } from "@/components/LoadProgress";
import { MapperSelectionTable } from "./components/MapperSelectionTable";
import { useAPI } from "@/hooks/useAPI";
import { archiveMappers, backupMappers } from "@/utility/fetch";
import { MapperUpdate } from "pokeaclient";
import { MapperFilesContext } from "@/Contexts/availableMapperContext";
import { OpenMapperFolderButton } from "@/components/OpenMapperFolderButton";
import { Advanced } from "@/components/Advanced";
import { WideButton } from "@/components/WideButton";
import { Panel } from "@/components/Panel";

export function MapperBackupPanel() {
	const filesClient = Store.client.files;
	const mapperFileContext = useContext(MapperFilesContext);
	const [availableMappers, setAvailableMappers] = useState<MapperUpdate[]>([]);
	const [selectedMappers, setSelectedMappers] = useState<string[]>([]);
	const archiveMappersApi = useAPI(archiveMappers, mapperFileContext.refresh);
	const backupApi = useAPI(backupMappers, mapperFileContext.refresh);
	// Load available mappers:

	// Process loaded mappers:
	useEffect(() => {
		setAvailableMappers(mapperFileContext.updates.filter(mapper => !!mapper.currentVersion) ?? []);
	}, [mapperFileContext.updates])

	const handleArchiveSelected = () => {
		const mappers = availableMappers
			.filter(x => selectedMappers.includes(x.currentVersion.path))
			.map(x => x.currentVersion);
		setSelectedMappers([]);
		archiveMappersApi.call(mappers);
	}
	const handleArchiveAll = () => {
		archiveMappersApi.call(availableMappers.map(x => x.currentVersion));
	}
	const handleBackupSelected = () => {
		const mappers = availableMappers
			.filter(x => selectedMappers.includes(x.currentVersion.path))
			.map(x => x.currentVersion);
		setSelectedMappers([]);
		backupApi.call(mappers);
	}
	const handleBackupAll = () => {
		backupApi.call(availableMappers.map(x => x.currentVersion));
	}

	if (mapperFileContext.isLoading) {
		return <LoadProgress label="Processing mapper(s)" />
	}
	return (
		<Panel id="mapper-backup" title="Backup mappers" >		
			<span>{selectedMappers.length} / {availableMappers.length} Mappers Selected</span>
			<div class="margin-top">
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
				<Advanced>
					<OpenMapperFolderButton />
				</Advanced>
			</div>
			<div class="margin-top">
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
				<Advanced>
					<WideButton 
						color="purple" 
						onClick={filesClient.openMapperArchiveFolder}
						text="Open archive folder"
					/>
				</Advanced>
			</div>
			<div class="margin-top">
				{archiveMappersApi.isLoading
					? <LoadProgress label="Archiving mapper(s)" />
					: <MapperSelectionTable
						availableMappers={availableMappers}
						selectedMappers={selectedMappers}
						onMapperSelection={setSelectedMappers}
						onUpdateList={mapperFileContext.refresh}
					/>
				}
			</div>
		</Panel>
	);
}
