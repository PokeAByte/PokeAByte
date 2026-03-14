import { useState,  useEffect } from "preact/hooks";
import { LoadProgress } from "../../../components/LoadProgress";
import { MapperSelectionTable } from "./components/MapperSelectionTable";
import { useAPI } from "../../../hooks/useAPI";
import { mapperFilesSignal, refreshMapperFiles } from "../../../Contexts/mapperFilesSignal";
import { OpenMapperFolderButton } from "../../../components/OpenMapperFolderButton";
import { Toasts } from "../../../notifications/ToastStore";
import { WideButton } from "../../../components/WideButton";
import { Panel } from "@/components/Panel";
import { Show } from "@preact/signals/utils";
import { advancedModeSignal } from "@/Contexts/uiSettingsSignal";
import { installMapper, MapperUpdate } from "@/utility/fetch";

export function DownloadMapperPanel() {
	const mapperFiles = mapperFilesSignal.value;
	const [downloads, setDownloads] = useState<MapperUpdate[]>([]);
	const [selectedDownloads, setSelectedDownloads] = useState<string[]>([]);
	const downloadMappers = useAPI(
		installMapper, 
		(_, success) => {
			if (success) {
				refreshMapperFiles();
				Toasts.push(`Successfully downloaded mapper(s).`, "task_alt", "green");
			}
		}
	);
	useEffect(() => {
		setDownloads(mapperFiles.updates.filter(mapper => !mapper.version) ?? []);
		setSelectedDownloads([]);		
	}, [mapperFiles.updates])

	const handleDownload = () => {
		const mappers = downloads.filter(x => selectedDownloads.includes(x.path));
		downloadMappers.call(mappers.map(x => x.path));
	}

	const handleDownloadAll = () => {
		downloadMappers.call(downloads.map(x => x.path));
	}

	if (downloadMappers.isLoading || mapperFiles.isLoading) {
		return <LoadProgress label="Downloading mapper(s)" />
	}
	return (
		<Panel id="mapper-download" title="Download mappers" >
			<span>
				{selectedDownloads.length} / {downloads.length} Mappers Selected
			</span>
			<div class="flexy-panel margin-top">
				<WideButton text="Download selected" color="green" disabled={!selectedDownloads.length} onClick={handleDownload}  />
				<WideButton text="Download all" color="green" disabled={!downloads.length} onClick={handleDownloadAll}  />
				<WideButton text="Reload mapper list" color="blue" disabled onClick={refreshMapperFiles}  />
				<Show when={advancedModeSignal}>
					<OpenMapperFolderButton />
				</Show>
			</div>
			<div class="margin-top">
				<MapperSelectionTable
					availableMappers={downloads}
					selectedMappers={selectedDownloads}
					onMapperSelection={setSelectedDownloads}
					availableHeader="Version"
					fallback={<strong>No mappers available for download.</strong>}
				/>
			</div>
		</Panel>
	);
}
