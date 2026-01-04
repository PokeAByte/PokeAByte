import { SelectInput } from "@/components/SelectInput";
import { Dropdown } from "@/components/Dropdown";
import { useContext, useEffect, useRef, useState } from "preact/hooks";
import { useAPI } from "@/hooks/useAPI";
import { LoadProgress } from "@/components/LoadProgress";
import { AvailableMapper, Mapper } from "pokeaclient";
import { MapperFilesContext } from "@/Contexts/availableMapperContext";
import { unique } from "@/utility/unique";
import { OpenMapperFolderButton } from "@/components/OpenMapperFolderButton";
import { Advanced } from "@/components/Advanced";
import { useStorageState } from "@/hooks/useStorageState";
import { changeMapper } from "@/utility/fetch";
import { createMapperLoadToast } from "./createMapperLoadToast";
import { Panel } from "@/components/Panel";

type MapperSelectProps = {
	mapper: Mapper | null
}

function createMapperOption(value: AvailableMapper) {
	return {
		value: value.id,
		display: value.displayName,
	};
}

export function LoadMapperPanel(props: MapperSelectProps) {
	const mapperFileContext = useContext(MapperFilesContext);
	const mapper = props.mapper;
	const loadButtonRef = useRef<HTMLButtonElement>(null)
	const changeMapperApi = useAPI(changeMapper, createMapperLoadToast);
	const [currentMapper, setCurrentMapper] = useState<string | null>(null);
	const [filter, setFilter] = useStorageState("mapper-category", "");

	useEffect(() => {
		if (currentMapper) {
			requestAnimationFrame(() => loadButtonRef.current?.focus());
		}
	}, [currentMapper]);

	useEffect(() => {
		setCurrentMapper(mapperFileContext.availableMappers?.find(x => x.id === mapper?.fileId)?.id ?? null);
	}, [mapper, mapperFileContext.availableMappers]);

	const onLoadMapper = () => {
		if (currentMapper) {
			changeMapperApi.call(currentMapper);
		}
	};

	if (changeMapperApi.isLoading) {
		return <LoadProgress label="Loading mapper" />
	}

	const availableCategories = [
		{ value: "", display: "<No filter>" },
		...mapperFileContext.availableMappers.map(x => x.displayName?.substring(1, x.displayName.indexOf(')')))
			.filter(unique)
			.toSorted()
			.map(x => ({ value: x, display: x }))
	];
	const filteredMappers = filter
		? mapperFileContext.availableMappers.filter(x => x.displayName.startsWith(`(${filter})`))
		: mapperFileContext.availableMappers;
	return (
		<Panel id="mapper-load" title="Load mapper" defaultOpen>			
			<span> Select the mapper you would like to load: </span>
			<br />
			<span class="margin-right">
				<Dropdown
					size={10}
					tabIndex={-1}
					id="mapper-select"
					onSelection={(option) => setFilter(option.value ?? "")}
					value={filter}
					options={availableCategories}
				/>
			</span>
			<SelectInput
				size={55}
				id="mapper-select"
				onSelection={(option) => setCurrentMapper(option.value)}
				value={currentMapper}
				options={filteredMappers.map(createMapperOption)}
			/>
			<button ref={loadButtonRef} class="green margin-left wide-button" onClick={onLoadMapper}>
				Load Mapper
			</button>
			<Advanced>
				<div class="margin-top">
					<OpenMapperFolderButton />
				</div>
			</Advanced>
		</Panel>
	);
}
