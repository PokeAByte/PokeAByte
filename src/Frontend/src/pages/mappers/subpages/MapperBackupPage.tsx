import { Store } from "../../../utility/propertyStore"
import { useState, useContext, useEffect } from "preact/hooks";
import { LoadProgress } from "../../../components/LoadProgress";
import { MapperSelectionTable } from "./components/MapperSelectionTable";
import { useAPI } from "../../../hooks/useAPI";
import { archiveMappers, backupMappers } from "../../../utility/fetch";
import { MapperUpdate } from "pokeaclient";
import { MapperFilesContext } from "../../../Contexts/availableMapperContext";
import { OpenMapperFolderButton } from "../../../components/OpenMapperFolderButton";
import { Advanced } from "../../../components/Advanced";

export function MapperBackupPage() {
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
		<article>
			<span>{selectedMappers.length} / {availableMappers.length} Mappers Selected</span>
			<div className="margin-top">
				<button className="green margin-right wide-button" disabled={!selectedMappers.length} onClick={handleBackupSelected}>
					Backup selected
				</button>
				<button className="margin-right green wide-button" disabled={!availableMappers.length} onClick={handleBackupAll}>
					Backup all
				</button>
				<Advanced>
					<OpenMapperFolderButton />
				</Advanced>
			</div>
			<div className="margin-top">
				<button 
					role="button"
					className="red margin-right wide-button" 
					disabled={selectedMappers.length === 0} 
					onClick={handleArchiveSelected}
				>
					Archive selected
				</button>
				<button 
					className="margin-right red wide-button" 
					disabled={availableMappers.length === 0} 
					onClick={handleArchiveAll}
				>
					Archive all
				</button>
				<Advanced>
					<button 
						className="purple wide-button" 
						onClick={filesClient.openMapperArchiveFolder}
					>
						Open archive folder
					</button>
				</Advanced>
			</div>
			<div className="margin-top">
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
		</article>
	);
}
