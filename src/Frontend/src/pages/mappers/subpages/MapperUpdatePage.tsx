import { Store } from "../../../utility/propertyStore"
import React, { useEffect } from "react";
import { LoadProgress } from "../../../components/LoadProgress";
import { MapperSelectionTable } from "./components/MapperSelectionTable";
import { useAPI } from "../../../hooks/useAPI";
import { MapperUpdate } from "pokeaclient";

export function MapperUpdatePage() {
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
			if (updateMappers.result) {
				setAvailableMappers(
					updateMappers.result
						.filter(mapper => !!mapper.currentVersion)
						.filter(mapper => !!mapper.latestVersion)
				);
			}
			setSelectedMappers([]);
		}
	}, [updateMappers.wasCalled, updateMappers.isLoading, updateMappers.result])

	useEffect(() => {
		if (downloadMappers.wasCalled && !downloadMappers.isLoading) {
			setSelectedMappers([]);
			downloadMappers.reset();
		}
	}, [downloadMappers, downloadMappers.wasCalled, downloadMappers.isLoading])

	const handleUpdate = () => {
		const mappers = availableMappers.filter(x => selectedMappers.includes(x.latestVersion.path));
		downloadMappers.call(mappers);
	}

	const handleUpdateAll = () => {
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
				<button className="border-green margin-right" disabled={!selectedMappers.length} onClick={handleUpdate}>
					UPDATE SELECTED
				</button>
				<button className="border-green margin-right" disabled={!availableMappers.length} onClick={handleUpdateAll}>
					UPDATE ALL
				</button>
				<button className="border-blue margin-right">
					CHECK FOR MAPPERS
				</button>
				<button className="border-purple" onClick={filesClient.openMapperFolder}>
					OPEN MAPPER FOLDER
				</button>
			</div>
			<MapperSelectionTable
				availableMappers={availableMappers}
				selectedMappers={selectedMappers}
				onMapperSelection={setSelectedMappers}
			/>
		</article>
	);
}
