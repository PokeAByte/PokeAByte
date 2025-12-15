import { Store } from "../../../utility/propertyStore"
import { useState, useContext, useEffect } from "preact/hooks";
import { LoadProgress } from "../../../components/LoadProgress";
import { MapperSelectionTable } from "./components/MapperSelectionTable";
import { useAPI } from "../../../hooks/useAPI";
import { MapperUpdate } from "pokeaclient";
import { MapperFilesContext } from "../../../Contexts/availableMapperContext";
import { OpenMapperFolderButton } from "../../../components/OpenMapperFolderButton";
import { Toasts } from "../../../notifications/ToastStore";
import { Advanced } from "../../../components/Advanced";
import { WideButton } from "../../../components/WideButton";
import { Panel } from "@/components/Panel";

export function DownloadMapperPanel() {
	const filesClient = Store.client.files;
	const mapperFileContext = useContext(MapperFilesContext);
	const [downloads, setDownloads] = useState<MapperUpdate[]>([]);
	const [selectedDownloads, setSelectedDownloads] = useState<string[]>([]);
	const downloadMappers = useAPI(
		filesClient.downloadMapperUpdatesAsync, 
		(success) => {
			if (success) {
				mapperFileContext.refresh();
				Toasts.push(`Successfully downloaded mapper(s).`, "task_alt", "green");
			} else {
				Toasts.push(`An error occured while downloading (a) mapper(s).`, "", "red");
			}
		}
	);
	useEffect(() => {
		setDownloads(mapperFileContext.updates.filter(mapper => !mapper.currentVersion) ?? []);
		setSelectedDownloads([]);		
	}, [mapperFileContext.updates])

	const handleDownload = () => {
		const mappers = downloads.filter(x => selectedDownloads.includes(x.latestVersion.path));
		downloadMappers.call(mappers);
	}

	const handleDownloadAll = () => {
		downloadMappers.call(downloads);
	}

	if (downloadMappers.isLoading || mapperFileContext.isLoading) {
		return <LoadProgress label="Downloading mapper(s)" />
	}
	return (
		<Panel id="mapper-download" title="Download mappers" >
			<span>
				{selectedDownloads.length} / {downloads.length} Mappers Selected
			</span>
			<div class="margin-top">
				<WideButton text="Download selected" color="green" disabled={!selectedDownloads.length} onClick={handleDownload}  />
				<WideButton text="Download all" color="green" disabled={!downloads.length} onClick={handleDownloadAll}  />
				<WideButton text="Reload mapper list" color="blue" disabled onClick={mapperFileContext.refresh}  />
				<Advanced>
					<OpenMapperFolderButton />
				</Advanced>
			</div>
			<div class="margin-top">
				<MapperSelectionTable
					availableMappers={downloads}
					selectedMappers={selectedDownloads}
					onMapperSelection={setSelectedDownloads}
				/>
			</div>
		</Panel>
	);
}
