import { Store } from "../../../utility/propertyStore"
import React, { useEffect } from "react";
import { LoadProgress } from "../../../components/LoadProgress";
import { MapperSelectionTable } from "./components/MapperSelectionTable";
import { useAPI } from "../../../hooks/useAPI";
import { archiveMappers } from "../../../utility/fetch";
import { MapperUpdate } from "pokeaclient";

export function MapperBackupPage() {
	const filesClient = Store.client.files;
	const [availableMappers, setAvailableMappers] = React.useState<MapperUpdate[]>([]);
	const [selectedMappers, setSelectedMappers] = React.useState<string[]>([]);
	const loadMappers = useAPI(filesClient.getMapperUpdatesAsync);
	const archiveMappersApi = useAPI(archiveMappers, loadMappers.call);
	const backupApi = useAPI(archiveMappers, loadMappers.call);
	// Load available mappers:
	useEffect(
		() => loadMappers.call(),
		// eslint-disable-next-line react-hooks/exhaustive-deps
		[]
	);

	// Process loaded mappers:
	useEffect(() => {
		if (loadMappers.isLoading === false && loadMappers.result) {
			setAvailableMappers(loadMappers.result?.filter(mapper => !!mapper.currentVersion) ?? []);
		}
	}, [loadMappers.isLoading, loadMappers.result])

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

	if (loadMappers.isLoading) {
		return <LoadProgress label="Downloading mapper(s)" />
	}
	return (
		<article>
			<span>{selectedMappers.length} / {availableMappers.length} Mappers Selected</span>
			<div className="margin-top">
				<button className="border-green margin-right" disabled={!selectedMappers.length} onClick={handleBackupSelected}>
					BACKUP SELECTED
				</button>
				<button className="margin-right border-green" disabled={!availableMappers.length} onClick={handleBackupAll}>
					BACKUP ALL
				</button>
				<button className="border-purple" onClick={filesClient.openMapperFolder}>
					OPEN MAPPER FOLDER
				</button>
			</div>
			<div className="margin-top">
				<button 
					role="button"
					className="border-red margin-right" 
					disabled={selectedMappers.length === 0} 
					onClick={handleArchiveSelected}
				>
					ARCHIVE SELECTED
				</button>
				<button 
					className="margin-right border-red" 
					disabled={availableMappers.length === 0} 
					onClick={handleArchiveAll}
				>
					ARCHIVE ALL
				</button>
				<button 
					className="border-blue" 
					onClick={filesClient.openMapperArchiveFolder}
				>
					OPEN ARCHIVE FOLDER
				</button>
			</div>
			<div className="margin-top">
			{archiveMappersApi.isLoading
				? <LoadProgress label="Archiving mapper(s)" />
				: <MapperSelectionTable
					availableMappers={availableMappers}
					selectedMappers={selectedMappers}
					onMapperSelection={setSelectedMappers}
					onUpdateList={() => loadMappers.call}
				/>
			}
			</div>
		</article>
	);
}

