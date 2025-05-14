import { Store } from "../../../utility/propertyStore"
import { useState, useContext, useEffect } from "preact/hooks";
import { LoadProgress } from "../../../components/LoadProgress";
import { MapperSelectionTable } from "./components/MapperSelectionTable";
import { useAPI } from "../../../hooks/useAPI";
import { MapperUpdate } from "pokeaclient";
import { MapperFilesContext } from "../../../Contexts/availableMapperContext";
import { OpenMapperFolderButton } from "../../../components/OpenMapperFolderButton";
import { Toasts } from "../../../notifications/ToastStore";
import { Advanced } from "../../../Contexts/Advanced";

export function MapperDownloadPage() {
	const filesClient = Store.client.files;
	const mapperFileContext = useContext(MapperFilesContext);
	const [downloads, setDownloads] = useState<MapperUpdate[]>([]);
	const [selectedDownloads, setSelectedDownloads] = useState<string[]>([]);
	const downloadMappers = useAPI(
		filesClient.downloadMapperUpdatesAsync, 
		(success) => {
			if (success) {
				mapperFileContext.refresh();
				Toasts.push(`Successfully downloaded mapper(s).`, "task_alt", "success");
			} else {
				Toasts.push(`An error occured while downloading (a) mapper(s).`, "", "error");
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
		<article>
			<span>
				{selectedDownloads.length} / {downloads.length} Mappers Selected
			</span>
			<div className="margin-top">
				<button className="green margin-right wide-button" disabled={!selectedDownloads.length} onClick={handleDownload}>
					Download selected
				</button>
				<button className="green margin-right wide-button" disabled={!downloads.length} onClick={handleDownloadAll}>
					Download all
				</button>
				<button className="blue margin-right wide-button" disabled onClick={mapperFileContext.refresh}>
					Reload mapper list
				</button>
				<Advanced>
					<OpenMapperFolderButton />
				</Advanced>
			</div>
			<div className="margin-top">
				<MapperSelectionTable
					availableMappers={downloads}
					selectedMappers={selectedDownloads}
					onMapperSelection={setSelectedDownloads}
				/>
			</div>
		</article>
	);
}
