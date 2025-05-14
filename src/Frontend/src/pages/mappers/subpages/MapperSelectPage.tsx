import { Store } from "../../../utility/propertyStore"
import { SelectInput } from "../../../components/SelectInput";
import { Dropdown } from "../../../components/Dropdown";
import { Toasts } from "../../../notifications/ToastStore";
import { useContext, useEffect, useRef, useState } from "preact/hooks";
import { useAPI } from "../../../hooks/useAPI";
import { LoadProgress } from "../../../components/LoadProgress";
import { useLocation } from "wouter";
import { AvailableMapper, Mapper } from "pokeaclient";
import { MapperFilesContext } from "../../../Contexts/availableMapperContext";
import { unique } from "../../propertyEditor/utils/unique";
import { OpenMapperFolderButton } from "../../../components/OpenMapperFolderButton";
import { Advanced } from "../../../Contexts/Advanced";
import { useStorageState } from "../../../hooks/useStorageState";

type MapperSelectProps = {
	mapper: Mapper | null
}

export function MapperSelection(props: MapperSelectProps) {
	const mapperFileContext = useContext(MapperFilesContext);
	const mapper = props.mapper;

	// @ts-expect-error The upstream type definition is incomplete, accessing fileId works just fine.
	const fileId = mapper?.fileId;
	const loadButtonRef = useRef<HTMLButtonElement>(null)
	const [, setLocation] = useLocation();
	const changeMapper = useAPI(Store.client.changeMapper);
	const [currentMapper, setCurrentMapper] = useState<string|null>(null);
	useEffect(() => {
		if (currentMapper) {
			requestAnimationFrame(() => loadButtonRef.current?.focus());
		}
	}, [currentMapper])
	const [filter, setFilter] = useStorageState("mapper-category", "");
	const onLoadMapper = () => {
		if (currentMapper) {
			changeMapper.call(currentMapper);
		}
	}

	useEffect(() => {
		setCurrentMapper(mapperFileContext.availableMappers?.find(x => x.id === fileId)?.id ?? null);
	}, [fileId, mapper,mapperFileContext.availableMappers])

	useEffect(() => {
		if (changeMapper.wasCalled) {
			if (!changeMapper.isLoading && changeMapper.result) {
				Toasts.push("Loaded mapper", "task_alt", "success");
				setLocation("../../properties");
			} else if (!changeMapper.isLoading && !changeMapper.result) {
				Toasts.push("Failed to load mapper", "", "error");
			}
		}
	}, [setLocation, changeMapper.wasCalled, changeMapper.isLoading, changeMapper.result])

	const onCategorySelect = (category: string|null ) => {
		let filter = category ?? "";
		if (category === "<No filter>") {
			filter = "";
		}
		setFilter(filter);
	}
	if (changeMapper.isLoading) {
		return <LoadProgress label="Loading mapper" />
	}
	const allMappers = mapperFileContext.availableMappers;
	const availableCategories = [
		{ value: "", display: "<No filter>" },
		...allMappers.map(x => x.displayName?.substring(1, x.displayName.indexOf(')')))
			.filter(unique)
			.toSorted()
			.map(x => ({ value: x, display: x }))
	];
	const filteredMappers = filter 
		? allMappers.filter(x => x.displayName.startsWith(`(${filter})`))
		: allMappers;
	return (
		<div>
			<span>
				Select the mapper you would like to load:
			</span>
			<br/>
			<span class={"margin-right"}>
				<Dropdown
					size={10}
					tabIndex={-1}
					placeholder="Select filter"
					id="mapper-select"
					onSelection={(option) => onCategorySelect(option.value)}
					value={filter}
					options={availableCategories}
				/>
			</span>
			<SelectInput
				size={55}
				id="mapper-select"
				onSelection={(option) => setCurrentMapper(option.value)}
				value={currentMapper}
				options={filteredMappers.map((x: AvailableMapper) => ({ value: x.id, display: x.displayName })) || []}
			/>
			<button ref={loadButtonRef} className="green margin-left wide-button" onClick={onLoadMapper}>
				Load Mapper
			</button>
			<Advanced>
				<div className="margin-top">
					<OpenMapperFolderButton />
				</div>
			</Advanced>
		</div>
	);
}