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
			<Panel title="Load mapper" >
				<MapperSelection mapper={mapper} mapperData={mapperData} />
			</Panel>
			<Panel title="Download mappers" >
				<MapperDownloadPage />
			</Panel>
			<Panel title="Update mappers" >
				<MapperUpdatePage />
			</Panel>
			<Panel title="Backup mappers" >
				<MapperBackupPage />
			</Panel>
			<Panel title="Restore backup/archive" >
				<MapperRestorePage />
			</Panel>
		</article>
	);
}

function Panel(props: {title: string, children: React.ReactNode}) {
	const [isOpen, setOpen] = useState(false);
	return (
		<details className="panel" onToggle={event => setOpen(event.currentTarget.open)}>
			<summary>{props.title}</summary>
			{ isOpen ? props.children : null}
		</details>
	);
}