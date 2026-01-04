import { useSyncExternalStore } from "preact/compat";
import { Store } from "@/utility/propertyStore";
import { LoadMapperPanel } from "./panels/LoadMapperPanel";
import { DownloadMapperPanel } from "./panels/MapperDownloadPage";
import { MapperBackupPanel } from "./panels/MapperBackupPanel";
import { UpdateMapperPanel } from "./panels/UpdateMapperPanel";
import { RestoreMapperPanel } from "./panels/MapperRestorePage";
import { FavoritePanel } from "./panels/FavoritePanel";
import { RecentPanel } from "./panels/RecentPanel";

export default function MapperPage() {
	const mapper = useSyncExternalStore(Store.subscribeMapper, Store.getMapper);
	
	return (
		<article class="margin-top">
			<FavoritePanel />
			<LoadMapperPanel mapper={mapper} />			
			<RecentPanel />				
			<DownloadMapperPanel />
			<UpdateMapperPanel />			
			<MapperBackupPanel />
			<RestoreMapperPanel />			
		</article>		
	);
}

export type PanelProps = {
	title: string, 
	defaultOpen?: boolean, 
	children: React.ReactNode
	id: string
}
