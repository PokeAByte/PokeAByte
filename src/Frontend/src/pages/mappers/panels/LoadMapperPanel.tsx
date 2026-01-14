import { SelectInput } from "@/components/SelectInput";
import { Dropdown } from "@/components/Dropdown";
import { useEffect, useRef, useState } from "preact/hooks";
import { useAPI } from "@/hooks/useAPI";
import { LoadProgress } from "@/components/LoadProgress";
import { mapperFilesSignal } from "@/Contexts/mapperFilesSignal";
import { unique } from "@/utility/unique";
import { OpenMapperFolderButton } from "@/components/OpenMapperFolderButton";
import { useStorageState } from "@/hooks/useStorageState";
import { changeMapper, MapperFile } from "@/utility/fetch";
import { onMapperLoaded as onMapperLoaded } from "./createMapperLoadToast";
import { Panel } from "@/components/Panel";
import { FavoriteIcon } from "./components/FavoriteIcon";
import { advancedModeSignal, saveSetting, uiSettingsSignal } from "@/Contexts/uiSettingsSignal";
import { Toasts } from "@/notifications/ToastStore";
import { mapperSignal } from "@/Contexts/mapperSignal";
import { useComputed } from "@preact/signals";
import { Show } from "@preact/signals/utils";
import { beautifyMapperName, getMapperCategory } from "@/utility/mapperName";

function createMapperOption(value: MapperFile) {
	return {
		value: value.path,
		display: beautifyMapperName(value),
		extra: <FavoriteIcon mapper={value} />,
	};
}

export function LoadMapperPanel() {
	const mapperFiles = mapperFilesSignal.value;
	const mapper = mapperSignal.value;
	const loadButtonRef = useRef<HTMLButtonElement>(null)
	const changeMapperApi = useAPI(changeMapper, onMapperLoaded);
	const [currentMapper, setCurrentMapper] = useState<string | null>(null);
	const [filter, setFilter] = useStorageState("mapper-category", "");
	const isRecentlyUsedEnabled = useComputed(() => uiSettingsSignal.value.recentlyUsedEnabled).value;
	const recentMappers = useComputed(() => uiSettingsSignal.value.recentMappers).value;

	useEffect(() => {
		if (currentMapper) {
			requestAnimationFrame(() => loadButtonRef.current?.focus());
		}
	}, [currentMapper]);

	useEffect(() => {
		setCurrentMapper(mapperFiles.availableMappers?.find(x => x.path === mapper?.path)?.path ?? null);
	}, [mapper, mapperFiles.availableMappers]);

	const onLoadMapper = () => {
		if (currentMapper) {
			Toasts.clearErrors();
			changeMapperApi.call(currentMapper);
			if (isRecentlyUsedEnabled) {
				saveSetting("recentMappers", [currentMapper, ...(recentMappers??[])].filter(unique).slice(0, 5));
			}
		}
	};

	if (changeMapperApi.isLoading) {
		return (
			<Panel id="mapper-load" title="Load mapper" defaultOpen>
				<LoadProgress label="Loading mapper" />
			</Panel>
		)
	}

	const availableCategories = [
		{ value: "", display: "<No filter>" },
		...mapperFiles.availableMappers.map(x => getMapperCategory(x.path))
			.filter(unique)
			.toSorted()
			.map(x => ({ value: x, display: x }))
	];
	const filteredMappers = filter
		? mapperFiles.availableMappers.filter(x => beautifyMapperName(x)?.startsWith(`(${filter})`))
		: mapperFiles.availableMappers;
	return (
		<Panel id="mapper-load" title="Load mapper" defaultOpen>			
			<span> Select the mapper you would like to load: </span>
			<br />
			<div class="flexy-panel">
				<Dropdown
					size={10}
					tabIndex={-1}
					onSelection={(option) => setFilter(option.value ?? "")}
					value={filter}
					options={availableCategories}
				/>
				<SelectInput
					size={45}
					onSelection={(option) => setCurrentMapper(option.value)}
					value={currentMapper}
					options={filteredMappers.map(createMapperOption)}
				/>
				<button ref={loadButtonRef} class="green wide-button" onClick={onLoadMapper}>
					Load Mapper
				</button>
				<Show when={advancedModeSignal}>
					<OpenMapperFolderButton />
				</Show>
			</div>
		</Panel>
	);
}
