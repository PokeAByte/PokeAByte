import { Store } from "../../../utility/propertyStore"
import React, { useEffect } from "react";
import { LoadProgress } from "../../../components/LoadProgress";
import { MapperSelectionTable } from "./components/MapperSelectionTable";
import { useAPI } from "../../../hooks/useAPI";
import { MapperUpdate } from "pokeaclient";

export function MapperDownloadPage() {
	const filesClient = Store.client.files;
	const [availableMappers, setAvailableMappers] = React.useState<MapperUpdate[]>([]);
	const [selectedMappers, setSelectedMappers] = React.useState<string[]>([]);
	const updateMappers = useAPI(filesClient.getMapperUpdatesAsync);
	const downloadMappers = useAPI(filesClient.downloadMapperUpdatesAsync, () => updateMappers.call);

	useEffect(
		() => updateMappers.call(),
		// eslint-disable-next-line react-hooks/exhaustive-deps
		[]
	);

	useEffect(() => {
		if (updateMappers.wasCalled && updateMappers.isLoading === false && updateMappers.result) {
			setAvailableMappers(updateMappers.result?.filter(mapper => !mapper.currentVersion) ?? []);
			setSelectedMappers([]);
		}
	}, [updateMappers.wasCalled, updateMappers.isLoading, updateMappers.result])

	const handleDownload = () => {
		const mappers = availableMappers.filter(x => selectedMappers.includes(x.latestVersion.path));
		downloadMappers.call(mappers);
	}

	const handleDownloadAll = () => {
		downloadMappers.call(availableMappers);
	}

	if (downloadMappers.isLoading || updateMappers.isLoading) {
		return <LoadProgress label="Downloading mapper(s)" />
	}
	return (
		<article>
			<span>
				{selectedMappers.length} / {availableMappers.length} Mappers Selected
			</span>
			<div className="margin-top">
				<button className="border-green margin-right" disabled={!selectedMappers.length} onClick={handleDownload}>
					DOWNLOAD SELECTED
				</button>
				<button className="border-green margin-right" disabled={!availableMappers.length} onClick={handleDownloadAll}>
					DOWNLOAD ALL
				</button>
				<button className="border-blue margin-right">CHECK FOR MAPPERS</button>
				<button className="border-purple" onClick={filesClient.openMapperFolder}>
					OPEN MAPPER FOLDER
				</button>
			</div>
			<div className="margin-top">
				<MapperSelectionTable
					availableMappers={availableMappers}
					selectedMappers={selectedMappers}
					onMapperSelection={setSelectedMappers}
				/>
			</div>
		</article>
	);
}
