import { LoadMapperPanel } from "./panels/LoadMapperPanel";
import { DownloadMapperPanel } from "./panels/MapperDownloadPage";
import { MapperBackupPanel } from "./panels/MapperBackupPanel";
import { UpdateMapperPanel } from "./panels/UpdateMapperPanel";
import { RestoreMapperPanel } from "./panels/MapperRestorePage";
import { FavoritePanel } from "./panels/FavoritePanel";
import { RecentPanel } from "./panels/RecentPanel";
import { ComponentChild } from "preact";

export default function MapperPage() {
	
	return (
		<article class="margin-top">
			<FavoritePanel />
			<LoadMapperPanel />			
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
	children: ComponentChild,
	id: string
}
