import { useEffect, useState, useSyncExternalStore } from "react";
import { Store } from "../../utility/propertyStore";
import { MapperSelection } from "./subpages/MapperSelectPage";
import { MapperDownloadPage } from "./subpages/MapperDownloadPage";
import { MapperBackupPage } from "./subpages/MapperBackupPage";
import { MapperUpdatePage } from "./subpages/MapperUpdatePage";
import { MapperRestorePage } from "./subpages/MapperRestorePage";
import { AvailableMapper } from "pokeaclient";

export default function MapperPage() {
	const mapper = useSyncExternalStore(Store.subscribeMapper, Store.getMapper);
	const [mapperData, setMapperData] = useState<AvailableMapper[] | null>(null);
	useEffect(() => {
		Store.client.getMappers().then(mappers => setMapperData(mappers));
	}, [])

	if (!mapperData) {
		return (null)
	}

	return (
		<article className="layout-box">
			<button className="border-red" disabled={!mapper} onClick={Store.client.unloadMapper}>
				{mapper
					? `Unload '${mapper?.gameName}'`
					: "No mapper loaded"
				}
			</button>
			<br/>
			<br/>
			<details open className="panel">
				<summary>Load mapper</summary>
				<MapperSelection mapper={mapper} mapperData={mapperData} />
			</details>
			<details className="panel">
				<summary>Download mappers</summary>
				<MapperDownloadPage />
			</details>
			<details className="panel">
				<summary>Update mappers</summary>
				<MapperUpdatePage />
			</details>
			<details className="panel">
				<summary>Backup mappers</summary>
				<MapperBackupPage />
			</details>
			<details className="panel">
				<summary>Restore backup/archive</summary>
				<MapperRestorePage />
			</details>
		</article>
	);
}