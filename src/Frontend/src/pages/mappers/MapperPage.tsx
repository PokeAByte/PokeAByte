import { useSyncExternalStore } from "preact/compat";
import { Store } from "../../utility/propertyStore";
import { MapperSelection } from "./subpages/MapperSelectPage";
import { MapperDownloadPage } from "./subpages/MapperDownloadPage";
import { MapperBackupPage } from "./subpages/MapperBackupPage";
import { MapperUpdatePage } from "./subpages/MapperUpdatePage";
import { MapperRestorePage } from "./subpages/MapperRestorePage";
import { MapperFilesContextProvider } from "../../Contexts/availableMapperContext";
import { useStorageState } from "../../hooks/useStorageState";

export default function MapperPage() {
	const mapper = useSyncExternalStore(Store.subscribeMapper, Store.getMapper);

	return (
		<MapperFilesContextProvider>
			<article className="layout-box margin-top">
				<Panel id="_mapper-select-panel" title="Load mapper" defaultOpen>
					<MapperSelection mapper={mapper} />
				</Panel>
				<Panel id="_mapper-download-panel" title="Download mappers" >
					<MapperDownloadPage />
				</Panel>
				<Panel id="_mapper-update-panel" title="Update mappers" >
					<MapperUpdatePage />
				</Panel>
				<Panel id="_mapper-backup-panel" title="Backup mappers" >
					<MapperBackupPage />
				</Panel>
				<Panel id="_mapper-restore-panel" title="Restore backup/archive" >
					<MapperRestorePage />
				</Panel>
			</article>
		</MapperFilesContextProvider>
	);
}

type PanelProps = {
	title: string, 
	defaultOpen?: boolean, 
	children: React.ReactNode
	id: string
}

function Panel(props: PanelProps) {
	const [isOpen, setOpen] = useStorageState(props.id, !!props.defaultOpen);
	return (
		<details 
			className="panel" 
			open={isOpen}
			onToggle={event => setOpen(event.currentTarget.open)}
		>
			<summary>{props.title}</summary>
			{ isOpen ? props.children : null}
		</details>
	);
}